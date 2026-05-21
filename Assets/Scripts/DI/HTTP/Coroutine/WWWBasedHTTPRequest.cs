using System;
using System.Collections;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Networking;

namespace DI.HTTP.Coroutine
{
    public class WWWBasedHTTPRequest : HTTPBaseRequestImpl, IHTTPRequest
    {
        public WWWBasedHTTPRequest(WWWBasedHTTPClient client)
            : base(client)
        {
        }

        public override IHTTPResponse performSync()
        {
            OnStart();

            UnityWebRequest wWW = UnityWebRequest.Get(getUrl());

            var webRequest = wWW.SendWebRequest();

            while (!webRequest.isDone)
            {
            }

            HTTPBaseResponseImpl hTTPBaseResponseImpl = new HTTPBaseResponseImpl(this);

            if (wWW.result == UnityWebRequest.Result.ConnectionError ||
                wWW.result == UnityWebRequest.Result.ProtocolError)
            {
                hTTPBaseResponseImpl.setStatusCode(parseStatusFromMessage(wWW.error));

                OnError(hTTPBaseResponseImpl, new HTTPException(wWW.error));
            }
            else
            {
                hTTPBaseResponseImpl.setStatusCode(200);

                byte[] responseBytes = null;

                if (wWW.downloadHandler != null)
                {
                    responseBytes = wWW.downloadHandler.data;
                }

                hTTPBaseResponseImpl.setDocument(new HTTPBaseDocumentImpl(responseBytes));

                OnSuccess(hTTPBaseResponseImpl);
            }

            OnComplete();

            wWW.Dispose();

            return hTTPBaseResponseImpl;
        }

        public override void performAsync()
        {
            WWWBasedHTTPFactory wWWBasedHTTPFactory = (WWWBasedHTTPFactory)getClient().getFactory();

            MonoBehaviour context = wWWBasedHTTPFactory.getContext();

            context.StartCoroutine(request());
        }

        public override bool validateCertificate(X509Certificate certificate, SslPolicyErrors sslPolicyErrors)
        {
            return sslPolicyErrors == SslPolicyErrors.None;
        }

        private IEnumerator request()
        {
            OnStart();

            UnityWebRequest www = UnityWebRequest.Get(getUrl());

            yield return www.SendWebRequest();

            HTTPBaseResponseImpl response = new HTTPBaseResponseImpl(this);

            if (www.result == UnityWebRequest.Result.ConnectionError ||
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                response.setStatusCode(parseStatusFromMessage(www.error));

                OnError(response, new HTTPException(www.error));
            }
            else
            {
                response.setStatusCode(200);

                byte[] responseBytes = null;

                if (www.downloadHandler != null)
                {
                    responseBytes = www.downloadHandler.data;
                }

                response.setDocument(new HTTPBaseDocumentImpl(responseBytes));

                OnSuccess(response);
            }

            OnComplete();

            www.Dispose();
        }

        protected int parseStatusFromMessage(string message)
        {
            int result = -1;

            if (message != null)
            {
                int num = message.IndexOf(' ');

                if (num != -1)
                {
                    try
                    {
                        result = int.Parse(message.Substring(0, num));
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return result;
        }
    }
}