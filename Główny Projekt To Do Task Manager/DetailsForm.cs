using System;
using System.Drawing; // Do kolorów
using System.Resources;
using System.Windows.Forms;

namespace Główny_Projekt_To_Do_Task_Manager
{
    public partial class DetailsForm : Form
    {

        // ---- FLAGI DLA FORM1 ---
        public bool IsDeleted { get; private set; } = false;
        public bool IsUpdated { get; private set; } = false;
        private TodoTask _currentTask; // Przechowuje aktualne zadanie

        // ---- Konstruktory ----

        // --- Konstruktor dla szczeółów zadania ---
        public DetailsForm(TodoTask task) // Konstruktor przyjmuje zadanie jako parametr
        {
            InitializeComponent();

            _currentTask = task; // Zapamiętujemy zadanie w zmiennej prywatnej

            // uzupelniam pola danymi z zadania otrzymanego
            lblTitle.Text = task.Title;
            lblPriority.Text = "Priorytet: " + task.Priority.ToString();
            lblStartDate.Text = "Data startu: " + task.Deadline.ToString("dd.MM.yyyy - HH:mm");
            lblStatus.Text = "Status: " + task.Status.ToString();
            lblDate.Text = "Termin: " + task.Deadline.ToString("dd.MM.yyyy - HH:mm");
           
            // Obsługa tagów
            if (task.Tags != null && task.Tags.Count > 0) // Sprawdzamy czy sa jakieś tagi
            {
                lblTags.Text = "Tagi: " + string.Join(", ", task.Tags);
            }
            else
            {
                lblTags.Text = "Tagi: brak";
            }

            // Opis
            txtDescription.Text = task.Description;

            // Kolorowanie priorytetu
            if (task.Priority == TaskPriority.High)
            {
                lblPriority.ForeColor = Color.Red;
            }

            UpdateStatusLabel();    // Aktualizuje label statusu
            UpdateCompleteButton(); // Aktualizuje przycisk (Complete/Unmark)

        }

        // ---- OBSŁUGA PRZYCISKÓW ----

        private void btnClose_Click(object sender, EventArgs e) // Przycisk zamknij
        {
            this.Close();
        }

        
        private void btnComplete_Click(object sender, EventArgs e) // Przycisk zakończ (Complete)
        {
            
            // -- Cheking co mamy robic --
            if (_currentTask.Status == TaskStatus.Completed) // Jeżeli jest już zakończone
            {
                // - Umark -
                _currentTask.Status = TaskStatus.NotCompleted;
                System.Media.SystemSounds.Beep.Play(); // Dzwiek windowsa
            }
            else
            {
                // - Complete -
                _currentTask.Status = TaskStatus.Completed;
                System.Media.SystemSounds.Beep.Play();
            }

            // Dane zmieniy sie
            IsUpdated = true;

            UpdateStatusLabel(); // Odświeżamy napis statusu
            UpdateCompleteButton(); // Odświeżamy przycisk
        }

        
        private void btnDelete_Click(object sender, EventArgs e) // Przycisk usuń (delete)
        {
            // Pytamy dla pewności
            DialogResult result = MessageBox.Show(
                "Czy na pewno chcesz usunąć to zadanie?\nTego nie da się cofnąć.",
                "Potwierdzenie",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Ustawiamy flagę usunięcia
                IsDeleted = true;
                this.Close();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e) // Przycisk edytuj (edit)
        {
            // Otwieramy AddTaskForm w trybie edycji, przekazując bieżące zadanie
            using (AddTaskForm editForm = new AddTaskForm(_currentTask))
            {
                DialogResult result = editForm.ShowDialog();

                if (result == DialogResult.OK)
                {
                    lblTitle.Text = _currentTask.Title;
                    lblPriority.Text = "Priorytet: " + _currentTask.Priority.ToString();
                    lblDate.Text = "Termin: " + _currentTask.Deadline.ToShortDateString();
                    txtDescription.Text = _currentTask.Description;
                    lblTags.Text = "Tagi: " + string.Join(", ", _currentTask.Tags);

                    // Ustawiamy flage ze cos sie zmieniło zeby Form1 tez sie odswiezyc
                    IsUpdated = true;

                    MessageBox.Show("Zadanie zaktualizowane!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // --- NOWA METODA: Aktualizacja wyglądu przycisku ---
        // ---- Metody ----
        private void UpdateCompleteButton() // Pomocnicza metoda do odświeżania przycisku zakończenia
        {
            if (_currentTask.Status == TaskStatus.Completed)
            {
                // Completed -> przycisk pozwala ODZNACZYĆ
                btnComplete.Text = "Odznacz";
                btnComplete.Image = null;
                btnComplete.TextAlign = ContentAlignment.MiddleCenter;
                btnComplete.BackColor = Color.Red; // Kolor ostrzegawczy/cofania
            }
            else
            {
                // Not Completed -> przycisk pozwala ZAKOŃCZYĆ
                btnComplete.Text = "Zakończ";
                btnComplete.Image = Properties.Resources.check_24dp_E3E3E3_FILL0_wght400_GRAD0_opsz24;
                btnComplete.BackColor = Color.ForestGreen;
            }
        }

        private void UpdateStatusLabel() // Pomocnicza metoda do odświeżania napisu statusu
        {
            lblStatus.Text = "Status: " + _currentTask.Status.ToString();

            // kolorowanie statusu
            if (_currentTask.Status == TaskStatus.Completed)
                lblStatus.ForeColor = Color.Green;
            else
                lblStatus.ForeColor = Color.Black;
        }

        private void DetailsForm_Load(object sender, EventArgs e)
        {

        }
    }

}