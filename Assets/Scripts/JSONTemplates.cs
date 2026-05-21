using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class JSONTemplates
{
	private static readonly HashSet<object> touched = new HashSet<object>();

	public static JSONObject TOJSON(object obj)
	{
		if (touched.Add(obj))
		{
			JSONObject obj2 = JSONObject.obj;
			FieldInfo[] fields = obj.GetType().GetFields();
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				JSONObject jSONObject = JSONObject.nullJO;
				if (!fieldInfo.GetValue(obj).Equals(null))
				{
					MethodInfo method = typeof(JSONTemplates).GetMethod("From" + fieldInfo.FieldType.Name);
					jSONObject = ((method != null) ? ((JSONObject)method.Invoke(null, new object[1]
					{
						fieldInfo.GetValue(obj)
					})) : ((fieldInfo.FieldType != typeof(string)) ? JSONObject.Create(fieldInfo.GetValue(obj).ToString()) : JSONObject.CreateStringObject(fieldInfo.GetValue(obj).ToString())));
				}
				if ((bool)jSONObject)
				{
					if (jSONObject.type != 0)
					{
						obj2.AddField(fieldInfo.Name, jSONObject);
					}
					else
					{
						UnityEngine.Debug.LogWarning("Null for this non-null object, property " + fieldInfo.Name + " of class " + obj.GetType().Name + ". Object type is " + fieldInfo.FieldType.Name);
					}
				}
			}
			PropertyInfo[] properties = obj.GetType().GetProperties();
			PropertyInfo[] array2 = properties;
			foreach (PropertyInfo propertyInfo in array2)
			{
				JSONObject jSONObject2 = JSONObject.nullJO;
				if (!propertyInfo.GetValue(obj, null).Equals(null))
				{
					MethodInfo method2 = typeof(JSONTemplates).GetMethod("From" + propertyInfo.PropertyType.Name);
					jSONObject2 = ((method2 != null) ? ((JSONObject)method2.Invoke(null, new object[1]
					{
						propertyInfo.GetValue(obj, null)
					})) : ((propertyInfo.PropertyType != typeof(string)) ? JSONObject.Create(propertyInfo.GetValue(obj, null).ToString()) : JSONObject.CreateStringObject(propertyInfo.GetValue(obj, null).ToString())));
				}
				if ((bool)jSONObject2)
				{
					if (jSONObject2.type != 0)
					{
						obj2.AddField(propertyInfo.Name, jSONObject2);
					}
					else
					{
						UnityEngine.Debug.LogWarning("Null for this non-null object, property " + propertyInfo.Name + " of class " + obj.GetType().Name + ". Object type is " + propertyInfo.PropertyType.Name);
					}
				}
			}
			return obj2;
		}
		UnityEngine.Debug.LogWarning("trying to save the same data twice");
		return JSONObject.nullJO;
	}

	public static Vector2 ToVector2(JSONObject obj)
	{
		float x = (!obj["x"]) ? 0f : obj["x"].f;
		float y = (!obj["y"]) ? 0f : obj["y"].f;
		return new Vector2(x, y);
	}

	public static JSONObject FromVector2(Vector2 v)
	{
		JSONObject obj = JSONObject.obj;
		if (v.x != 0f)
		{
			obj.AddField("x", v.x);
		}
		if (v.y != 0f)
		{
			obj.AddField("y", v.y);
		}
		return obj;
	}

	public static JSONObject FromVector3(Vector3 v)
	{
		JSONObject obj = JSONObject.obj;
		if (v.x != 0f)
		{
			obj.AddField("x", v.x);
		}
		if (v.y != 0f)
		{
			obj.AddField("y", v.y);
		}
		if (v.z != 0f)
		{
			obj.AddField("z", v.z);
		}
		return obj;
	}

	public static Vector3 ToVector3(JSONObject obj)
	{
		float x = (!obj["x"]) ? 0f : obj["x"].f;
		float y = (!obj["y"]) ? 0f : obj["y"].f;
		float z = (!obj["z"]) ? 0f : obj["z"].f;
		return new Vector3(x, y, z);
	}

	public static JSONObject FromVector4(Vector4 v)
	{
		JSONObject obj = JSONObject.obj;
		if (v.x != 0f)
		{
			obj.AddField("x", v.x);
		}
		if (v.y != 0f)
		{
			obj.AddField("y", v.y);
		}
		if (v.z != 0f)
		{
			obj.AddField("z", v.z);
		}
		if (v.w != 0f)
		{
			obj.AddField("w", v.w);
		}
		return obj;
	}

	public static Vector4 ToVector4(JSONObject obj)
	{
		float x = (!obj["x"]) ? 0f : obj["x"].f;
		float y = (!obj["y"]) ? 0f : obj["y"].f;
		float z = (!obj["z"]) ? 0f : obj["z"].f;
		float w = (!obj["w"]) ? 0f : obj["w"].f;
		return new Vector4(x, y, z, w);
	}

	public static JSONObject FromMatrix4x4(Matrix4x4 m)
	{
		JSONObject obj = JSONObject.obj;
		if (m.m00 != 0f)
		{
			obj.AddField("m00", m.m00);
		}
		if (m.m01 != 0f)
		{
			obj.AddField("m01", m.m01);
		}
		if (m.m02 != 0f)
		{
			obj.AddField("m02", m.m02);
		}
		if (m.m03 != 0f)
		{
			obj.AddField("m03", m.m03);
		}
		if (m.m10 != 0f)
		{
			obj.AddField("m10", m.m10);
		}
		if (m.m11 != 0f)
		{
			obj.AddField("m11", m.m11);
		}
		if (m.m12 != 0f)
		{
			obj.AddField("m12", m.m12);
		}
		if (m.m13 != 0f)
		{
			obj.AddField("m13", m.m13);
		}
		if (m.m20 != 0f)
		{
			obj.AddField("m20", m.m20);
		}
		if (m.m21 != 0f)
		{
			obj.AddField("m21", m.m21);
		}
		if (m.m22 != 0f)
		{
			obj.AddField("m22", m.m22);
		}
		if (m.m23 != 0f)
		{
			obj.AddField("m23", m.m23);
		}
		if (m.m30 != 0f)
		{
			obj.AddField("m30", m.m30);
		}
		if (m.m31 != 0f)
		{
			obj.AddField("m31", m.m31);
		}
		if (m.m32 != 0f)
		{
			obj.AddField("m32", m.m32);
		}
		if (m.m33 != 0f)
		{
			obj.AddField("m33", m.m33);
		}
		return obj;
	}

	public static Matrix4x4 ToMatrix4x4(JSONObject obj)
	{
		Matrix4x4 result = default(Matrix4x4);
		if ((bool)obj["m00"])
		{
			result.m00 = obj["m00"].f;
		}
		if ((bool)obj["m01"])
		{
			result.m01 = obj["m01"].f;
		}
		if ((bool)obj["m02"])
		{
			result.m02 = obj["m02"].f;
		}
		if ((bool)obj["m03"])
		{
			result.m03 = obj["m03"].f;
		}
		if ((bool)obj["m10"])
		{
			result.m10 = obj["m10"].f;
		}
		if ((bool)obj["m11"])
		{
			result.m11 = obj["m11"].f;
		}
		if ((bool)obj["m12"])
		{
			result.m12 = obj["m12"].f;
		}
		if ((bool)obj["m13"])
		{
			result.m13 = obj["m13"].f;
		}
		if ((bool)obj["m20"])
		{
			result.m20 = obj["m20"].f;
		}
		if ((bool)obj["m21"])
		{
			result.m21 = obj["m21"].f;
		}
		if ((bool)obj["m22"])
		{
			result.m22 = obj["m22"].f;
		}
		if ((bool)obj["m23"])
		{
			result.m23 = obj["m23"].f;
		}
		if ((bool)obj["m30"])
		{
			result.m30 = obj["m30"].f;
		}
		if ((bool)obj["m31"])
		{
			result.m31 = obj["m31"].f;
		}
		if ((bool)obj["m32"])
		{
			result.m32 = obj["m32"].f;
		}
		if ((bool)obj["m33"])
		{
			result.m33 = obj["m33"].f;
		}
		return result;
	}

	public static JSONObject FromQuaternion(Quaternion q)
	{
		JSONObject obj = JSONObject.obj;
		if (q.w != 0f)
		{
			obj.AddField("w", q.w);
		}
		if (q.x != 0f)
		{
			obj.AddField("x", q.x);
		}
		if (q.y != 0f)
		{
			obj.AddField("y", q.y);
		}
		if (q.z != 0f)
		{
			obj.AddField("z", q.z);
		}
		return obj;
	}

	public static Quaternion ToQuaternion(JSONObject obj)
	{
		float x = (!obj["x"]) ? 0f : obj["x"].f;
		float y = (!obj["y"]) ? 0f : obj["y"].f;
		float z = (!obj["z"]) ? 0f : obj["z"].f;
		float w = (!obj["w"]) ? 0f : obj["w"].f;
		return new Quaternion(x, y, z, w);
	}

	public static JSONObject FromColor(Color c)
	{
		JSONObject obj = JSONObject.obj;
		if (c.r != 0f)
		{
			obj.AddField("r", c.r);
		}
		if (c.g != 0f)
		{
			obj.AddField("g", c.g);
		}
		if (c.b != 0f)
		{
			obj.AddField("b", c.b);
		}
		if (c.a != 0f)
		{
			obj.AddField("a", c.a);
		}
		return obj;
	}

	public static Color ToColor(JSONObject obj)
	{
		Color result = default(Color);
		for (int i = 0; i < obj.Count; i++)
		{
			switch (obj.keys[i])
			{
			case "r":
				result.r = obj[i].f;
				break;
			case "g":
				result.g = obj[i].f;
				break;
			case "b":
				result.b = obj[i].f;
				break;
			case "a":
				result.a = obj[i].f;
				break;
			}
		}
		return result;
	}

	public static JSONObject FromLayerMask(LayerMask l)
	{
		JSONObject obj = JSONObject.obj;
		obj.AddField("value", l.value);
		return obj;
	}

	public static LayerMask ToLayerMask(JSONObject obj)
	{
		LayerMask result = default(LayerMask);
		result.value = (int)obj["value"].n;
		return result;
	}

	public static JSONObject FromRect(Rect r)
	{
		JSONObject obj = JSONObject.obj;
		if (r.x != 0f)
		{
			obj.AddField("x", r.x);
		}
		if (r.y != 0f)
		{
			obj.AddField("y", r.y);
		}
		if (r.height != 0f)
		{
			obj.AddField("height", r.height);
		}
		if (r.width != 0f)
		{
			obj.AddField("width", r.width);
		}
		return obj;
	}

	public static Rect ToRect(JSONObject obj)
	{
		Rect result = default(Rect);
		for (int i = 0; i < obj.Count; i++)
		{
			switch (obj.keys[i])
			{
			case "x":
				result.x = obj[i].f;
				break;
			case "y":
				result.y = obj[i].f;
				break;
			case "height":
				result.height = obj[i].f;
				break;
			case "width":
				result.width = obj[i].f;
				break;
			}
		}
		return result;
	}

	public static JSONObject FromRectOffset(RectOffset r)
	{
		JSONObject obj = JSONObject.obj;
		if (r.bottom != 0)
		{
			obj.AddField("bottom", r.bottom);
		}
		if (r.left != 0)
		{
			obj.AddField("left", r.left);
		}
		if (r.right != 0)
		{
			obj.AddField("right", r.right);
		}
		if (r.top != 0)
		{
			obj.AddField("top", r.top);
		}
		return obj;
	}

	public static RectOffset ToRectOffset(JSONObject obj)
	{
		RectOffset rectOffset = new RectOffset();
		for (int i = 0; i < obj.Count; i++)
		{
			switch (obj.keys[i])
			{
			case "bottom":
				rectOffset.bottom = (int)obj[i].n;
				break;
			case "left":
				rectOffset.left = (int)obj[i].n;
				break;
			case "right":
				rectOffset.right = (int)obj[i].n;
				break;
			case "top":
				rectOffset.top = (int)obj[i].n;
				break;
			}
		}
		return rectOffset;
	}

	public static AnimationCurve ToAnimationCurve(JSONObject obj)
	{
		AnimationCurve animationCurve = new AnimationCurve();
		if (obj.HasField("keys"))
		{
			JSONObject field = obj.GetField("keys");
			for (int i = 0; i < field.list.Count; i++)
			{
				animationCurve.AddKey(ToKeyframe(field[i]));
			}
		}
		if (obj.HasField("preWrapMode"))
		{
			animationCurve.preWrapMode = (WrapMode)obj.GetField("preWrapMode").n;
		}
		if (obj.HasField("postWrapMode"))
		{
			animationCurve.postWrapMode = (WrapMode)obj.GetField("postWrapMode").n;
		}
		return animationCurve;
	}

	public static JSONObject FromAnimationCurve(AnimationCurve a)
	{
		JSONObject obj = JSONObject.obj;
		obj.AddField("preWrapMode", a.preWrapMode.ToString());
		obj.AddField("postWrapMode", a.postWrapMode.ToString());
		if (a.keys.Length > 0)
		{
			JSONObject jSONObject = JSONObject.Create();
			for (int i = 0; i < a.keys.Length; i++)
			{
				jSONObject.Add(FromKeyframe(a.keys[i]));
			}
			obj.AddField("keys", jSONObject);
		}
		return obj;
	}

	public static Keyframe ToKeyframe(JSONObject obj)
	{
		Keyframe result = new Keyframe((!obj.HasField("time")) ? 0f : obj.GetField("time").n, (!obj.HasField("value")) ? 0f : obj.GetField("value").n);
		if (obj.HasField("inTangent"))
		{
			result.inTangent = obj.GetField("inTangent").n;
		}
		if (obj.HasField("outTangent"))
		{
			result.outTangent = obj.GetField("outTangent").n;
		}
		return result;
	}

	public static JSONObject FromKeyframe(Keyframe k)
	{
		JSONObject obj = JSONObject.obj;
		if (k.inTangent != 0f)
		{
			obj.AddField("inTangent", k.inTangent);
		}
		if (k.outTangent != 0f)
		{
			obj.AddField("outTangent", k.outTangent);
		}
		if (k.time != 0f)
		{
			obj.AddField("time", k.time);
		}
		if (k.value != 0f)
		{
			obj.AddField("value", k.value);
		}
		return obj;
	}
}
