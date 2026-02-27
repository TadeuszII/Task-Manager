using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq; // Potrzebne do obsługi list i tagów
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Główny_Projekt_To_Do_Task_Manager
{
    public partial class AddTaskForm : Form
    {
        // ---- Zmienna do przechowywania nowego lub edytowanego zadania ----
        public TodoTask NewTask { get; private set; } // Danne, do głównego okna

        private bool _isEditMode = false; // Flaga edycji aby korzystac z add task form do edycji

        // ---- Konstruktory ----

        // --- KONSTRUKTOR Dla dodanie zadania ---
        public AddTaskForm() // Pierwszy konsturktor - dodawanie nowego zadania
        {
            InitializeComponent();
            
            cmbPriority.SelectedIndex = 1; // Ustawiam domyślnie Priorytet na "Medium" 
            _isEditMode = false;
        }

        // --- KONSTRUKTOR dla edycji zadania ---
        public AddTaskForm(TodoTask taskToEdit)
        {
            InitializeComponent();

            _isEditMode = true;
            NewTask = taskToEdit; // Pracujemy na tym konkretnym zadaniu
            this.Text = "Edytuj zadanie"; // Zmieniamy tytuł okna

            // Wypełniamy pola danymi z zadania
            txtTitle.Text = taskToEdit.Title;
            txtDescription.Text = taskToEdit.Description;
            dtpDeadline.Value = taskToEdit.Deadline;

            // Ustawiamy ComboBoxa
            cmbPriority.SelectedItem = taskToEdit.Priority.ToString();

            // Zamieniamy listę tagów z powrotem na tekst "a, b, c"
            if (taskToEdit.Tags != null)
            {
                txtTags.Text = string.Join(", ", taskToEdit.Tags);
            }
        }

        // ---- Przysciski ----
        private void btnCancel_Click(object sender, EventArgs e) // Obsługa przycisku ANULUJ
        {
            // Ustawiamy wynik na Cancel i zamykamy
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        
        private void btnSave_Click(object sender, EventArgs e) // Obsługa przycisku ZAPISZ
        {
            // ... (Twoja walidacja txtTitle i cmbPriority zostaje bez zmian) ...
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Proszę podać tytuł!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            // Jezelei to nowe zadanie -> TWORZYMY OBIEKT
            if (!_isEditMode)
            {
                NewTask = new TodoTask();
            }

            // AKTUALIZUJEMY DANE
            NewTask.Title = txtTitle.Text;
            NewTask.Description = txtDescription.Text;
            NewTask.Deadline = dtpDeadline.Value;

            string selectedPriority = cmbPriority.SelectedItem.ToString();
            NewTask.Priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), selectedPriority);

            // Tagi - czyścimy stare i dodajemy nowe
            NewTask.Tags.Clear(); // Ważne przy edycji!
            if (!string.IsNullOrWhiteSpace(txtTags.Text))
            {
                string[] tagsArray = txtTags.Text.Split(',');
                foreach (string t in tagsArray)
                {
                    string cleanTag = t.Trim();
                    if (!string.IsNullOrEmpty(cleanTag))
                    {
                        NewTask.Tags.Add(cleanTag);
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
