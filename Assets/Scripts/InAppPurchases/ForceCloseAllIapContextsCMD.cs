using UnityEngine;

namespace InAppPurchases
{
	public class ForceCloseAllIapContextsCMD
	{
		public void Execute()
		{
		IAPContext[] array = Object.FindObjectsByType<IAPContext>(FindObjectsSortMode.None);
			IAPContext[] array2 = array;
			foreach (IAPContext iAPContext in array2)
			{
				iAPContext.Close();
			}
		}
	}
}
