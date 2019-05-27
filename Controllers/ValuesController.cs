using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using JulioCesarApi.Models;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;

namespace JulioCesarApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private static readonly string baseUrl = "https://api.codenation.dev/v1/challenge/dev-ps/generate-data?token=879f225b199ec8284cbaf5b80d7ac334f1327200";
        /*
            {
                "numero_casas":1,
                "token":"879f225b199ec8284cbaf5b80d7ac334f1327200",
                "cifrado":"gpdvt jt tbzjoh op up 1000 hppe jefbt. tufwf kpct",
                "decifrado":"",
                "resumo_criptografico":""
            } 
        */
        // GET api/values/
        [HttpGet]
        public ActionResult<Desafio> Get()
        {
            var value = GetDesafio();
            value.decifrado = JCDecode(value, 1);
            using (var sha1 = new SHA1Managed())
            {
                value.resumo_criptografico = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(value.decifrado)));
            }
            string json = JsonConvert.SerializeObject(value, Formatting.Indented);
            
            System.IO.File.WriteAllText("answer.json", json);
            
            return value;
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] Desafio obj)
        {
            obj.decifrado = JCDecode(obj, 1);
            using (var sha1 = new SHA1Managed())
            {
                obj.resumo_criptografico = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(obj.decifrado)));
            }
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            
            System.IO.File.WriteAllText("answer.json", json);
        }

        // Put api/values
        [HttpPut]
        public void Put([FromBody] Desafio json)
        {
        }

        private string JCDecode(Desafio json, int pos)
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

        private Desafio GetDesafio()
        {
            var result = new Desafio();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsAsync<Desafio>().Result;
            }
            return result;
        }

        private void PostDesafio()
        {
            var result = new Desafio();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync(baseUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                result = response.Content.ReadAsAsync<Desafio>().Result;
            }
            ;
        }
    }
}
