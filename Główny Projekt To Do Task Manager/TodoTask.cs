using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Główny_Projekt_To_Do_Task_Manager
{
    // ---- Enum dla Priorytetu zadania ----
    public enum TaskPriority // Wartości dla Priorytetu
    {
        Low,
        Medium,
        High
    }

    // ---- Enum dla Statusu zadania ----
    public enum TaskStatus // Wartości dla Statusu
    {
        NotCompleted,
        Completed,
        Expired
    }

    public class TodoTask
    {
        // ---- Podstawowe właściwości zadania ----
        public string Id { get; set; } // Unikalny identyfikator (niezmienny)

        public string Title { get; set; } // Tytuł zadania (wymagany)

        public string Description { get; set; } // Opis zadania

        public TaskPriority Priority { get; set; } // Priorytet (Z listy enum)

        public TaskStatus Status { get; set; } //  Status zadania

        public List<string> Tags { get; set; } // Tagi (lista napisów)

        // ---- Daty ----
        public DateTime StartDate { get; set; }
        public DateTime Deadline { get; set; }

        // ---- Konstruktor ----
        public TodoTask()
        {
            
            Id = Guid.NewGuid().ToString(); // Unikalny kod ID

            //  --- Wartości domyślne ---
            Tags = new List<string>();
            StartDate = DateTime.Now;      // Dziś
            Deadline = DateTime.Now.AddDays(1); // Jutro (domyślnie)
            Status = TaskStatus.NotCompleted;
            Priority = TaskPriority.Medium;
        }
    }
}
