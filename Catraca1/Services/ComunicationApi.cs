using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using dotenv.net;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace CatracaControlClient.Services
{
    public class ComunicationApi
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private const string baseUri = "https://api-v2.intranet.maracanau.ifce.edu.br/v1/integracao/transito/";
        private const string usuario = "c4tr4c4";
        private const string senha= "YzR0cjRjNHA0c3N3MHJk";
        
        //inicializa as variaveis de backup
        public class BackupUsers
        {
            public string Clearcode {get;set;}
        }
        //verifica se o cartão é válido
        public async Task<bool> GetUser(string dadosTag,string tipoMovimentacao, string numeroCatraca)
        {
            try
            {
                var requestUri = new Uri($"{baseUri}tag?cartao={dadosTag}&tipoMovimentacao={tipoMovimentacao}&numeroCatraca={numeroCatraca}&usuario={usuario}&senha={senha}");

                //Mostra valor da variável requestUri
                //System.Console.WriteLine($"GET: {requestUri}");
                
                HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                //Mostra valor da variável response
                //System.Console.WriteLine($"Get: {response.StatusCode}");

                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                    var autorizacao = JObject.Parse(responseBody);
                    if((int)autorizacao["autorizacao"] == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    
                }
                else
                {
                    Console.WriteLine($"Erro ao validar usuario API");
                    string fileContent = File.ReadAllText("backup_users.json");
                    List<BackupUsers> backup_users = JsonConvert.DeserializeObject<List<BackupUsers>>(fileContent);
                    if(backup_users.Any(user => user.Clearcode == dadosTag))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção autorizar usuario {ex.Message}");
                string fileContent = File.ReadAllText("backup_users.json");
                List<BackupUsers> backup_users = JsonConvert.DeserializeObject<List<BackupUsers>>(fileContent);
                if(backup_users.Any(user => user.Clearcode == dadosTag))
                {
                    return true;
                }
                else
                {
                    return false;
                }
                
            }
        }
        //Posta os dados usuario que passou na catraca no banco
        public async Task<bool> PostUser(string dadosTag, string numCatraca, string tipoMovimentacao, string dataHora)
        {
            try
            {
                var queryString = $"inserirtransito?cartao={dadosTag}&numeroCatraca={numCatraca}&tipoMovimentacao={tipoMovimentacao}&data_hora={dataHora}&usuario={usuario}&senha={senha}";
                
                var requestUri = new Uri($"{baseUri}{queryString}");

                //Mostra valor da variável requestUri
                //System.Console.WriteLine($"POST: {requestUri}");

                var response = await httpClient.PostAsync(requestUri, null);

                //Mostra valor da variável response
                //System.Console.WriteLine($"Post: {response.StatusCode}");
                
                if(response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    
                    Console.WriteLine($"Erro ao postar usuario");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção postar usuario: {ex.Message}");
                return false;
            }
        }
        // realiza o backup dos cartões dos usuarios
        public async Task<List<BackupUsers>> GetBackupUsers()
        {
            try
            {
                string requestUri = ($"{baseUri}backup?usuario={usuario}&senha={senha}");

                HttpResponseMessage response = await httpClient.GetAsync(requestUri);

                if(response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    List<BackupUsers> backupUsers = JsonConvert.DeserializeObject<List<BackupUsers>>(responseBody);
                    File.WriteAllText("backup_users.json", JsonConvert.SerializeObject(backupUsers));
                    return backupUsers;                    
                }
                else
                {
                    Console.WriteLine($"Erro ao fazer backup dos usuarios");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao fazer backup dos usuarios: {ex.Message}");
                return null;
            }
        }
    }
}

