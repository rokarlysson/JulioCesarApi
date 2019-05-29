using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using JulioCesarApi.Models;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace JulioCesarApi
{
    class Program
    {
        private static readonly string token = "879f225b199ec8284cbaf5b80d7ac334f1327200";
        private static readonly string getUrl = "generate-data?token=";
        private static readonly string postUrl = "submit-solution?token=";
        private static readonly string baseUrl = "https://api.codenation.dev/v1/challenge/dev-ps/";
        /*
            {
                "numero_casas":1,
                "token":"879f225b199ec8284cbaf5b80d7ac334f1327200",
                "cifrado":"gpdvt jt tbzjoh op up 1000 hppe jefbt. tufwf kpct",
                "decifrado":"",
                "resumo_criptografico":""
            } 
        */
        static void Main(string[] args)
        {
            var obj = GetFileAsDesafio();
            Console.WriteLine(WriteDesafioFile(obj));
            Console.WriteLine(WriteTextoDecifrado());
            Console.WriteLine(WriteResumoCripto());
            Console.WriteLine(PostJson());
        }

        private static Desafio GetFileAsDesafio()
        {
            Desafio obj = null;
            if (!File.Exists("answer.json"))
                obj = JsonConvert.DeserializeObject<Desafio>(GetJson());
            else
                obj = JsonConvert.DeserializeObject<Desafio>(System.IO.File.ReadAllText("answer.json"));
            return obj;
        }

        private static string WriteDesafioFile(Desafio obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            System.IO.File.WriteAllText("answer.json", json);
            return json;
        }
        
        private static string WriteTextoDecifrado()
        {
            var obj = JsonConvert.DeserializeObject<Desafio>(System.IO.File.ReadAllText("answer.json"));
            obj.decifrado = JCDecode(obj, 1);
            var json = WriteDesafioFile(obj);
            return json;
        }

        private static string WriteResumoCripto()
        {
            var obj = JsonConvert.DeserializeObject<Desafio>(System.IO.File.ReadAllText("answer.json"));
            using (var sha1 = new SHA1Managed())
            {
                obj.resumo_criptografico = GenerateHashString(sha1, obj.decifrado);
            }
            string json = WriteDesafioFile(obj);
            return json;
        }

        private static string GenerateHashString(HashAlgorithm algo, string text)
        {
            // Compute hash from text parameter
            algo.ComputeHash(Encoding.UTF8.GetBytes(text));
            // Get has value in array of bytes
            var result = algo.Hash;
            // Return as hexadecimal string
            return string.Join(string.Empty, result.Select(x => x.ToString("x2")));
        }

        private static string JCDecode(Desafio json, int pos)
        {
            var textoDecrypt = string.Empty;
            // Itera sobre o array de caracteres da string e obtendo o código ASCII voltando a posição passada,
            // descriptografando o texto ignorando espaços, pontos e números
            for (int i = 0; i < json.cifrado.Length; i++)
            {
                int codASCII = (int)json.cifrado[i];
                int keyASCII = 0;

                if (!char.IsLetter(json.cifrado[i]) || json.cifrado[i] == ' ' || json.cifrado[i] == '.')
                    keyASCII = codASCII;
                else
                    keyASCII = codASCII - pos;

                textoDecrypt += Char.ConvertFromUtf32(keyASCII);
            }

            return textoDecrypt;
        }

        private static string GetJson()
        {
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                var uri = new Uri(baseUrl + getUrl + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                result = response.Content.ReadAsStringAsync().Result;
            }
            return result;
        }

        private static string PostJson()
        {
            var result = string.Empty;
            using (var client = new HttpClient())
            {
                var uri = new Uri(baseUrl + postUrl + token);
                using (var content = new MultipartFormDataContent("DesafioCN----" + DateTime.Now.ToString(System.Globalization.CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new FileStream("answer.json", FileMode.Open)), "answer", "answer.json");
                    using (HttpResponseMessage response = client.PostAsync(uri, content).Result)
                    {
                        result = response.Content.ReadAsStringAsync().Result;
                    }
                }
            }
            return result;
        }
    }
}