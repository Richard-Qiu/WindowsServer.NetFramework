using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace WindowsServer.Utlitity
{
    public class SerializeXmlUtil
    {
        /// <summary>
        /// 从某一XML文件反序列化到某一类型
        /// </summary>
        /// <param name="filePath">待反序列化的XML文件名称</param>
        /// <returns></returns>
        public static T DeserializeFromXml<T>(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {

                    using (var reader = new StreamReader(filePath))
                    {
                        var xs = new XmlSerializer(typeof(T));
                        var ret = (T)xs.Deserialize(reader);
                        return ret;
                    }
                }
            }
            catch (Exception)
            {
                throw;
                //igorne
            }

            return default(T);
        }

        /// <summary>
        /// 从XML字符串中反序列化对象
        /// </summary>
        /// <typeparam name="T">结果对象类型</typeparam>
        /// <param name="xml">包含对象的XML字符串</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>反序列化得到的对象</returns>
        public static T DeserializeXml<T>(string xml, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException("s");
            }

            if (encoding == null)
            {
                encoding = UTF8Encoding.UTF8;
            }

            XmlSerializer mySerializer = new XmlSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(xml)))
            {
                using (StreamReader sr = new StreamReader(ms, encoding))
                {
                    return (T)mySerializer.Deserialize(sr);
                }
            }
        }

        /// <summary>
        /// 将一个对象按XML序列化的方式写入到一个文件
        /// </summary>
        /// <param name="o">要序列化的对象</param>
        /// <param name="path">保存文件路径</param>
        /// <param name="encoding">编码方式</param>
        public static void XmlSerializeToFile(object o, string path, Encoding encoding = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            if (encoding == null)
            {
                encoding = UTF8Encoding.UTF8;
            }

            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                XmlSerializeInternal(file, o, encoding);
            }
        }

        private static void XmlSerializeInternal(Stream stream, object o, Encoding encoding)
        {
            if (o == null)
                throw new ArgumentNullException("object could not be null");

            if (encoding == null)
                throw new ArgumentNullException("encoding could not be null");

            XmlSerializer serializer = new XmlSerializer(o.GetType());

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineChars = "\r\n";
            settings.Encoding = encoding;
            settings.IndentChars = "    ";

            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                serializer.Serialize(writer, o);
                writer.Close();
            }
        }
    }
}
