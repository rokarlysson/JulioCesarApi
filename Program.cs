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

namespace JulioCesarApi
{
    class Program
    {
        private static readonly string token = "879f225b199ec8284cbaf5b80d7ac334f1327200";
        private static readonly string getUrl = "generate-data?token=";
        private static readonly string postUrl = "generate-data?token=";
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
            Console.WriteLine(JsonConvert.SerializeObject(GetDesafio(), Formatting.Indented));
        }

        private static Desafio GetDesafio()
        {
            var value = JsonConvert.DeserializeObject<Desafio>(GetJson());
            value.decifrado = JCDecode(value, 1);
            using (var sha1 = new SHA1Managed())
            {
                value.resumo_criptografico = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(value.decifrado)));
            }
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            
            System.IO.File.WriteAllText("answer.json", json);
            
            return value;
        }

        private static string JCDecode(Desafio json, int pos)
        {
            var textoDecrypt = string.Empty;
            /*  
                Itera sobre o array de caracteres da string e obtendo o
                código ASCII voltando a posição passada, descriptografando
                o texto ignorando espaços, pontos e números
            */
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
            var uri = baseUrl + getUrl + token;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsStringAsync().Result;
            }
            return result;
        }
    }
}
