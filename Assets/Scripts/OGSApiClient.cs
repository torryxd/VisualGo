using System;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

public class OGSApiClient : MonoBehaviour
{
    private HttpClient _client;
    private readonly string _apiBaseUrl = "https://online-go.com/api/v1/";

    private async void Start()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_apiBaseUrl);
        _client.DefaultRequestHeaders.Accept.Clear();
        _client.DefaultRequestHeaders.Add("User-Agent", "YourAppName");


        string username = "1423324"; // Reemplaza con el nombre de usuario real
        string userData = await GetUserDataAsync(username);
        if (userData != null)
        {
            // Procesa la información del usuario (userData) como desees
            Debug.Log(userData);
        }
        else
        {
            Debug.LogError("No se pudo obtener la información del usuario.");
        }
    }

    public async Task<string> GetUserDataAsync(string username)
    {
        try
        {
            // Realiza una solicitud GET a la API de OGS para obtener datos del usuario
            HttpResponseMessage response = await _client.GetAsync($"players/{username}/");

            // Verifica si la solicitud fue exitosa
            if (response.IsSuccessStatusCode)
            {
                // Lee la respuesta como una cadena JSON
                string json = await response.Content.ReadAsStringAsync();
                return json;
            }
            else
            {
                // Maneja el caso en que la solicitud no fue exitosa
                return null;
            }
        }
        catch (Exception ex)
        {
            // Maneja errores de solicitud
            Debug.LogError("Error: " + ex.Message);
            return null;
        }
    }

    public async Task<string> GetWinnerAsync(int[,] board) //
    {
        try
        {
            // Serializa el tablero como JSON
            string boardJson = JsonUtility.ToJson(board);

            // Crea el contenido de la solicitud
            var content = new StringContent(boardJson, Encoding.UTF8, "application/json");

            // Realiza una solicitud POST a la API de OGS para obtener el ganador del tablero
            HttpResponseMessage response = await _client.PostAsync("determine-winner", content);

            // Verifica si la solicitud fue exitosa
            if (response.IsSuccessStatusCode)
            {
                // Lee la respuesta como una cadena JSON que contiene al ganador
                string winnerJson = await response.Content.ReadAsStringAsync();
                return winnerJson;
            }
            else
            {
                // Maneja el caso en que la solicitud no fue exitosa
                return null;
            }
        }
        catch (Exception ex)
        {
            // Maneja errores de solicitud
            Console.WriteLine("Error: " + ex.Message);
            return null;
        }
    }


}