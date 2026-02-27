using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // Potrzebne do obsługi plików
using Newtonsoft.Json; // Potrzebne do obsługi JSON

namespace Główny_Projekt_To_Do_Task_Manager
{
    public class TaskService
    {

        // ---- Path do pliku bazy danych ----
        private string filePath = "database.json"; // Ścieżka do naszego pliku bazy danych

        // ---- Metody do obsługi bazy danych ----

        public bool DatabaseExists() // --- Metoda sprawdzająca, czy baza danych istnieje ---
        {
            return File.Exists(filePath);
        }

        
        public void CreateNewDatabase() // ---  Metoda tworząca pustą bazę (jeśli użytkownik tak zdecyduje) ---
        {
            List<TodoTask> emptyList = new List<TodoTask>();
            SaveTasks(emptyList);
        }

        
        public void SaveTasks(List<TodoTask> tasks) // --- Metoda Write liste do pliku ---
        {
            string json = JsonConvert.SerializeObject(tasks, Formatting.Indented); // Zamienia liste obiektow na tekst w formacie JSON

            File.WriteAllText(filePath, json); // Zapisujemy tekst do pliku (overides it)
        }

        
        public List<TodoTask> LoadTasks() // Metoda Read liseę z pliku
        {
            
            if (!DatabaseExists()) // Jeśli plik nie istnieje zwracamy pustą lista aby program nie wyletil z bledem
            {
                return new List<TodoTask>();
            }

            
            string json = File.ReadAllText(filePath); // Wczytuje tresc pliku do stringu

            var tasks = JsonConvert.DeserializeObject<List<TodoTask>>(json); // Zamieaniamy tekst z powrotem na lista objektow TodoTask
            return tasks ?? new List<TodoTask>();
        }
    }
}
