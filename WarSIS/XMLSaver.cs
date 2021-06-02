using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Collections.Generic;

namespace WarSIS
{
    
    static class XMLSaver
    {
        public static Boolean CreateNew(Dictionary<String, Dictionary<String, String>> Table, String Path, out String Error)
        {
            Error = "";
            try
            {
                // создаём XML документ в памяти
                var xdoc = new XDocument();
                // определяем корневой элемент
                XElement data = new XElement("data");
                // для каждой строки таблицы ...
                foreach (var Item in Table)
                {
                    var Key = new XElement(Item.Key);
                    foreach (var Field in Item.Value)
                    {
                        Key.Add(new XAttribute(Field.Key, Field.Value));
                    }
                    data.Add(Key);
                }
                // добавляем корневой элемент в XML-документ
                xdoc.Add(data);
                // сохраняем XML-документ
                xdoc.Save(Path);
                return true;
            }
            catch (Exception e)
            {
                // если произошла ошибка, то выводим текст ошибки
                Error = e.Message;
                return false;
            }
        }


        public static Boolean Save(Dictionary<String, Dictionary<String, String>> Table, String Path, out String Error)
        {
            Error = "";
            if (!File.Exists(Path))
               return CreateNew(Table, Path, out Error);
            else
            {
                try
                {
                    // создаём XML документ в памяти
                    var xDoc = new XmlDocument();
                    // загружаем XML-документ
                    xDoc.Load(Path);
                    // обращаемся к корневому элементу XML-документа
                    XmlElement data = xDoc.DocumentElement;
                    foreach (var Item in Table)
                    {
                        var Keys = data.GetElementsByTagName(Item.Key);
                        if(Keys.Count > 0)
                            data.RemoveChild(Keys[0]);
                        var Key = xDoc.CreateElement(Item.Key);
                        if(Item.Value != null)
                            foreach (var Field in Item.Value)
                            {
                                var name = xDoc.CreateAttribute(Field.Key);
                                var value = xDoc.CreateTextNode(Field.Value);
                                name.AppendChild(value);
                                Key.Attributes.Append(name);
                            }
                        data.AppendChild(Key);
                    }
                    xDoc.Save(Path);
                    return true;
                }
                catch (Exception e)
                {
                    // если произошла ошибка, то выводим текст ошибки
                    Error = e.Message;
                    return false;
                }
            }
        }

        public static Boolean Remove(String Key, String Path, out String Error)
        {
            Error = "";
            try
            {
                // создаём XML документ в памяти
                var xDoc = new XmlDocument();
                // загружаем XML-документ
                xDoc.Load(Path);
                // обращаемся к корневому элементу XML-документа
                XmlElement data = xDoc.DocumentElement;
                var Keys = data.GetElementsByTagName(Key);
                if (Keys.Count > 0)
                    data.RemoveChild(Keys[0]);
                xDoc.Save(Path);
                return true;
            }
            catch (Exception e)
            {
                // если произошла ошибка, то выводим текст ошибки
                Error = e.Message;
                return false;
            }
        }

        public static Boolean Load(out Dictionary<String, String> Table, String Path, String Key, out String Error)
        {
            // инициализируем таблицу
            Table = new Dictionary<string, string>();
            Error = "";
            try
            {
                // создаём XML документ в памяти
                var xDoc = new XmlDocument();
                // загружаем XML-документ
                xDoc.Load(Path);
                // обращаемся к корневому элементу XML-документа
                XmlElement xRoot = xDoc.DocumentElement;
                // для каждого субэлемента корневого элемента ...
                foreach (XmlNode xnode in xRoot)
                {
                    if (Key.Length > 0)
                    {
                        if (xnode.Name.CompareTo(Key) == 0)
                            foreach (XmlNode attrib in xnode.Attributes)
                            {
                                Table.Add(attrib.Name, attrib.Value);
                            }
                    } else
                    {
                        Table.Add(xnode.Name, xnode.InnerText);
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                // если произошла ошибка, то выводим текст ошибки
                Error = e.Message;
                return false;
            }
        }
    }

}
