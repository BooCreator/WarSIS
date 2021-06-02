using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using WarSISDataBase.DataBase;

using WarSISModelsDB.Models;

namespace WarSIS
{
    public static class Ext
    {
        // Функция генерации SHA1-хэша для строки.
        public static String SHA1(String str)
            => Encoding.UTF8.GetString(getHash(Encoding.UTF8.GetBytes(str))).Replace("'", "\"");
        // Функция генерации SHA1-хэша для массива байт
        private static byte[] getHash(byte[] bytes)
        {
            using (var sha = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hash = sha.ComputeHash(sha.ComputeHash(bytes));
                return hash;
            }
        }

        public static T Create<T>(IDataBaseEditor DataBase) where T : class, IDataBaseElement
        {
            Type Type = typeof(T);
            object Class = Activator.CreateInstance(Type);
            var Item = (Class as T);
            Item.Editor = DataBase;
            return Item;
        }

        public static bool? MessageBox(String Message, String Title = "", MessageBoxButton Buttons = MessageBoxButton.OK) 
            => new SharedForms.MessageBox(Message, Title, Buttons).ShowDialog();

        public static byte[] ImageToBytes(BitmapImage image)
        {
            byte[] data = null;
            if(image != null)
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                using(MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    data = ms.ToArray();
                }
            }
            return data;
        }
        public static BitmapImage BytesToImage(byte[] array)
        {
            var Image = new BitmapImage();
            if(array != null)
            {
                using(var ms = new MemoryStream(array))
                {

                    Image.BeginInit();
                    Image.CacheOption = BitmapCacheOption.OnLoad;
                    Image.StreamSource = ms;
                    Image.EndInit();

                }
            }
            return Image;
        }
    }

    public static class Extension
    {
        public static List<String> ToStringList(this ItemCollection Items)
        {
            List<string> Result = new List<string>();
            foreach (var Item in Items)
                Result.Add(Item.ToString());
            return Result;
        }

        public static Object Find(this ItemCollection Items, String Value)
        {
            foreach(var Item in Items)
                if(Item.ToString().CompareTo(Value) == 0)
                    return Item;
            return null;
        }
    }
}
