using System;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Sceelix.Utils
{
    public static class JTokenExtensions
    {
        public static T ToTypeOrDefault<T>(this JToken token)
        {
            if (token != null)
                return token.ToObject<T>();

            return default(T);
        }
        

        public static Vector2 ToVector2(this JToken jToken)
        {
            var coordinates = jToken.ToString().Split(',');

            return new Vector3(Convert.ToSingle(coordinates[0], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[1], CultureInfo.InvariantCulture));
        }

        public static Vector3 ToVector3(this JToken jToken)
        {
            var coordinates = jToken.ToString().Split(',');

            return new Vector3(Convert.ToSingle(coordinates[0], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[1], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[2], CultureInfo.InvariantCulture));
        }

        public static Vector4 ToVector4(this JToken jToken)
        {
            var coordinates = jToken.ToString().Split(',');

            return new Vector4(Convert.ToSingle(coordinates[0], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[1], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[2], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[3], CultureInfo.InvariantCulture));
        }

        public static Color ToColor(this JToken jToken)
        {
            var coordinates = jToken.ToString().Split(',');

            return new Color(Convert.ToSingle(coordinates[0], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[1], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[2], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[3], CultureInfo.InvariantCulture));
        }


        public static Quaternion ToQuaternion(this JToken jToken)
        {
            var coordinates = jToken.ToString().Split(',');

            return Quaternion.Euler(Convert.ToSingle(coordinates[0], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[1], CultureInfo.InvariantCulture), Convert.ToSingle(coordinates[2], CultureInfo.InvariantCulture));
        }

        public static T ToEnum<T>(this JToken jToken)
        {
            String enumString = jToken.ToObject<String>();
            return (T)Enum.Parse(typeof(T), enumString);
        }

        
        public static Texture2D ToTexture(this JToken jToken, bool setAsNormal = false)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, true);// {alphaIsTransparency = true}

            byte[] bytes = jToken.ToObject<byte[]>();
            
            texture2D.LoadImage(bytes);
            
            //to support proper alpha cutouts without white borders
            texture2D.ToPremultipliedTexture();

            if (setAsNormal)
            {
                texture2D = texture2D.ToNormalTexture();
            }

            return texture2D;
        }
    }
}
