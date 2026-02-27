using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Dla chartu

namespace Główny_Projekt_To_Do_Task_Manager
{
    public partial class Form1 : Form
    {
        // ---- Zmienne globalne ----

        
        private TaskService taskService; // do zarządzania plikami

        
        private List<TodoTask> currentTasks; // Lista, zadań

        bool sidebarExpand = true; // stan menu: true = rozwinięte, false = zwinięte


        // ---- Konstruktory ----
        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true; // Zmniejsza migotanie przy animacji sidebara

            InitializeApp(); // logika startowa
        }


        // ---- METODY STARTOWE ----
        private void InitializeApp() 
        {
            
            taskService = new TaskService(); // Tworzymy 

            InitializeFilters(); // ---- UStaw filty ---- // Nowy kod

            if (!taskService.DatabaseExists()) // SPRAWDZANIE BAZY DANYCH 
            {
                // Pytamy użytkownika, co robić
                DialogResult result = MessageBox.Show(
                    "Nie znaleziono pliku bazy danych (database.json).\nCzy chcesz utworzyć nowy plik?",
                    "Brak bazy danych",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Tworzymy pusta baza
                    taskService.CreateNewDatabase();
                    MessageBox.Show("Utworzono nową bazę danych.", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Jesli uzytkownik nacisni nie to zakrywamy program
                    MessageBox.Show("Aplikacja zostanie zamknięta.", "Koniec", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Environment.Exit(0); // Exit z programu
                }
            }

            // Wczytujemy zadania (z pliku, ktory juz na pewno istnieje)
            currentTasks = taskService.LoadTasks();

            CheckForExpiredTasks(); // Sprawdzamy  czy sa jakieś expired zadania
            UpdateTagFilterList(); // Update lista tagów w filtrze
            RefreshDataGrid(); // Metoda do wyświetlania zadań w datagridview
        }

        
        private void InitializeFilters() // Inicjalizacja filtrów
        {
            // Combo box Status
            cmbFilterStatus.Items.Clear(); // Czyscimy aby nie bylo duplikatow
            cmbFilterStatus.Items.Add("Wszystkie");

            // Pobieramy nazwy z Enuma TaskStatus z TodoTask.cs
            foreach (var status in Enum.GetNames(typeof(TaskStatus)))
            {
                cmbFilterStatus.Items.Add(status);
            }
            cmbFilterStatus.SelectedIndex = 0; // Domyślnie "Wszystkie" Gdy odkrywamy program

            // Combo box Priorytety

            cmbFilterPriority.Items.Clear();
            cmbFilterPriority.Items.Add("Wszystkie");

            // Pobieramy nazwy z Enuma TaskPriority
            foreach (var priority in Enum.GetNames(typeof(TaskPriority)))
            {
                cmbFilterPriority.Items.Add(priority);
            }
            cmbFilterPriority.SelectedIndex = 0; // Domyślnie "Wszystkie"
        }


        // --- Metoda refreszeowanie dannych w tablice ---
        private void RefreshDataGrid(List<TodoTask> tasksToDisplay = null) // Domyślna wartość = null (jeżeli nic nie podać jako parametr wyświetli currentTasks)
        {

            //jeżeli nic nie podać jako parametr wyświetli currentTasks
            if (tasksToDisplay == null)
            {
                tasksToDisplay = currentTasks;
            }

            dgvTasks.Rows.Clear(); //Wyczysc tabele (aby ne dublowac wierszy)

            
            foreach (var task in tasksToDisplay) // Przelecimy pętlą przez każde zadanie z naszej listy
            {
                
                string tagsText = string.Join(", ", task.Tags); // laczymy tagi w jeden string skleja przecinkami



                // Dodajemy do data grid view dane muzsa byc po kolei ja w designerze
                int rowIndex = dgvTasks.Rows.Add( // Dodajemy nowy wiersz do tabeli (Tytuł, Priorytet, Deadline, Status, Tagi)
                    task.Title,
                    task.Priority,
                    task.Deadline.ToString("yyyy-MM-dd HH:mm"), 
                    task.Status,
                    tagsText
                );

                
                
                dgvTasks.Rows[rowIndex].Tag = task.Id;  // Ukrywam ID wiersza .Tag konteiner gdzie mozna shcowac dowolny objekt

                // --- KOLOROWANIE WIERSZY ---
                if (task.Status == TaskStatus.Completed)
                {
                    // Zakończone na zielony
                    dgvTasks.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                }
                else if (task.Status == TaskStatus.Expired)
                {
                    // Expired na jasno czerwony
                    dgvTasks.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Salmon;
                }


            }
            CheckForExpiredTasks(); // -- mozliwe jest overused
        }


        // ----- <OBSŁUGA MENU BOCZNEGO> -----

        // --- Timer do animacji sidebaru ---
        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            
            //  minimalną i maksymalną szerokość
            int minWidth = 50;   // Tyle, żeby było widać tylko ikony 
            int maxWidth = 250;  // Pełna szerokość
            int step = 10;        // Krok zmiany szerokości przy każdym ticku

            if (sidebarExpand)
            {
                // Jeśli jest rozwinięte, to zwijamy
                panelSidebar.Width -= step; 

                if (panelSidebar.Width <= minWidth)
                {
                    sidebarExpand = false; // flaga
                    sidebarTimer.Stop();   // skończyliśmy ruch
                    panelContent.Visible = true; // OPTYMALIZACJA!
                }
            }
            else
            {
                // Jeśli jest zwinięte, to rozwijamy
                panelSidebar.Width += step; // Zwiększamy szerokość

                if (panelSidebar.Width >= maxWidth)
                {
                    sidebarExpand = true; // flagae
                    sidebarTimer.Stop();  // Zatrzymujemy timer

                    panelContent.Visible = true; // OPTYMALIZACJA!
                }
            }
            
        }


        // --- Przyciski menu bocznego ---
        private void btnMenu_Click(object sender, EventArgs e) // Przycisk menu bocznego Czyli odkrywa zakrywa menu boczne
        {
            // OPTYMALIZACJA: Ukrywamy ciężką zawartość przed startem animacji
            panelContent.Visible = false;

            sidebarTimer.Start(); // W metodzie sidebarTimer_Tick będzie obsługa animacji
        }

        private void btnMain_Click(object sender, EventArgs e) // Panel Zadania
        {
            panelStatistics.Visible = false; // Ukryj statystyki
            dgvTasks.Visible = true;         // Pokaż tabelę
            panelAbout.Visible = false;   // Ukryj about
            panelSettings.Visible = false; // Ukryj ustawienia 
            

            RefreshDataGrid(); // Odśwież tabela na wypadek zmian
            
        }

        private void btnStatistics_Click(object sender, EventArgs e) // Panel Statystyki
        {
            // --- Aktualizacja statystyk ---
            UpdateStatistics();

            panelStatistics.Visible = true; // Pokaż statystyki
            dgvTasks.Visible = false;         // Ukryj tabelę
            panelAbout.Visible = false;   // Ukryj about
            panelSettings.Visible = false; // Ukryj ustawienia 

        }

        private void btnSettings_Click(object sender, EventArgs e) // Panel Ustawienia
        {
            panelStatistics.Visible = false; // Ukryj statystyki
            dgvTasks.Visible = false;         // Ukryj tabelę
            panelAbout.Visible = false;   // Ukryj about
            panelSettings.Visible = true; // Pokaż ustawienia 
        }

        private void btnAbout_Click(object sender, EventArgs e) // Panel About
        {
            panelStatistics.Visible = false; // Ukryj statystyki
            dgvTasks.Visible = false;         // Ukryj tabelę
            panelAbout.Visible = true;   // Pokaż about
            panelSettings.Visible = false; // Ukryj ustawienia 

        }

        // ----- <Paneli> -----

        // ---- [Panel Zadania] ----

        // --- Przyciski ---
        private void btnAdd_Click(object sender, EventArgs e) // Przycisk Dodaj Zadanie
        {

            //using gwarantuje że gdy tylko zamkniemy okienko dodawania pamięć po nim zostanie natychmiast "cleared" przez system
            using (AddTaskForm form = new AddTaskForm()) // Tworzymy i pokazujemy formularz dodawania jako "Dialog" (okno blokujące)
            {
                DialogResult result = form.ShowDialog(); // Pokazujemy formularz jako okno (blokujące)


                if (result == DialogResult.OK) // Sprawdzamy, czy użytkownik kliknął "Zapisz" (DialogResult.OK)
                {
                    
                    TodoTask newTask = form.NewTask; // Pobieramy nowe zadanie z formularza

                    
                    currentTasks.Add(newTask); // Dodajemy je do naszej listy w pamięci

                    
                    taskService.SaveTasks(currentTasks); // Zapisujemy całą zaktualizowaną listę do pliku
                    
                    UpdateTagFilterList(); // Aktualizujemy listę tagów w filtrze (bo mogły się pojawić nowe)

                    // Message potwierdzenie (zanim zrobimy tabelke)
                    MessageBox.Show($"Dodano zadanie: {newTask.Title}\nZapisano w bazie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    RefreshDataGrid(); // Odświeżamy tabelę, żeby pokazać nowe zadanie
                }
            }
        }

        private void dgvTasks_CellDoubleClick(object sender, DataGridViewCellEventArgs e) // Podwójne kliknięcie w wiersz tabeli
        {
            
                if (e.RowIndex >= 0) // Sprawdzamy, czy użytkownik nie kliknął w nagłówek (wiersz -1)
                {
                
                    string taskId = dgvTasks.Rows[e.RowIndex].Tag.ToString(); // ukryte ID z wiersza, w który kliknięto

                
                    // Find szuka elementu spełniającego warunek (t.Id == taskId)
                    TodoTask selectedTask = currentTasks.Find(t => t.Id == taskId); // Szukamy zadania o tym ID w liście currentTasks | lambda funkcja czyli krótka funkcja sprawdzająca, czy t.Id równa się taskId.
                    // Find: szuka elementu spełniającego warunek (t.Id == taskId)


                
                if (selectedTask != null) // Sprawdzamy czy znaleźliśmy zadanie (na wszelki wypadek)
                    {
                    
                        using (DetailsForm details = new DetailsForm(selectedTask)) //Otwieramy okno szczegółów i przekazujemy znalezione zadanie
                        {
                            details.ShowDialog();

                            // --- SPRAWDZAMY CO SIĘ STAŁO PO ZAMKNIĘCIU ---

                            // Wariant 1: Użytkownik deletnul zadanie
                            if (details.IsDeleted)
                            {
                                currentTasks.Remove(selectedTask); // Usuwamy z listy currentTask
                                taskService.SaveTasks(currentTasks); // Zapisujemy zmiany w pliku
                                UpdateTagFilterList(); // Aktualizuje listę tagów w filtrze (tag mógl być usunięty)
                                RefreshDataGrid(); // Refresz tabelę
                            }
                            // Wariant 2: Użytkownik zmienił status (Zakończ)
                            else if (details.IsUpdated)
                            {
                                taskService.SaveTasks(currentTasks);
                                UpdateTagFilterList(); // Aktualizujemy listę tagów 
                                RefreshDataGrid(); // Refresz tabelę
                        }
                        }



                    }
                }
            }

        private void btnSearch_Click(object sender, EventArgs e) // Przycisk Szukaj
        {
            PerformSearch();
        }

        private void btnResetFilters_Click(object sender, EventArgs e)
        {
            
            txtSearch.Text = string.Empty; // Czyścimy pasek wyszukiwania
            
            if (cmbFilterTags.Items.Count > 0) // filtry na pierwszą pozycję("Wszystkie")
                cmbFilterTags.SelectedIndex = 0;

            if (cmbFilterStatus.Items.Count > 0) // filtry na pierwszą pozycję("Wszystkie")
                cmbFilterStatus.SelectedIndex = 0;

            if (cmbFilterPriority.Items.Count > 0) // filtry na pierwszą pozycję("Wszystkie")
                cmbFilterPriority.SelectedIndex = 0;

            PerformSearch();
        }

        // -- SORTOWANIE --

        bool directionPriority = true;
        bool directionDate = true;
        bool directionStatus = true;

        // - ALGORYTM BĄBELKOWY (Bubble Sort) - PO Priorytetu -
        private void btnSortPriority_Click(object sender, EventArgs e) // Sort bombelkowy po priotytetach
        {

            int n = currentTasks.Count;
            bool swapped; // Flaga czy w ogóle coś zamieniliśmy
            directionPriority = !directionPriority; // False - rosnąco True malejąco Toggle switch 

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;
                // Pętla wewnętrzna: porównuje sąsiadów
                for (int j = 0; j < n - i - 1; j++)
                {
                    
                    // Enum ma wartości: Low=0, Medium=1, High=2.
                    if (directionPriority == false) // Toggle switch sortujemy w obu stronach malejąco i rosnąco
                    {
                        if (currentTasks[j].Priority < currentTasks[j + 1].Priority)
                        {
                            TodoTask temp = currentTasks[j];
                            currentTasks[j] = currentTasks[j + 1];
                            currentTasks[j + 1] = temp;

                            swapped = true;
                        }
                    }
                    else
                    {
                        if (currentTasks[j].Priority > currentTasks[j + 1].Priority)
                        {
                            TodoTask temp = currentTasks[j];
                            currentTasks[j] = currentTasks[j + 1];
                            currentTasks[j + 1] = temp;

                            swapped = true;
                        }
                    }

                }

                // Check flagi jezeli przesli po calej liscie i nic nie zmenilo sie to robimy break
                if (!swapped)
                    break;
            }

            //RefreshDataGrid();
            PerformSearch(); // meniam bo refresh data grid nie uwzglednia tagi i poszukiwanie
        }

        // - ALGORYTM BĄBELKOWY (Bubble Sort) - PO DACIE -
        private void btnSortDate_Click(object sender, EventArgs e)
        {
            
            int n = currentTasks.Count;
            directionDate = !directionDate; // False - rosnąco True malejąco Toggle switch 
            //bool swapped = false; // Flaga czy w ogóle coś zamieniliśmy
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (directionDate == false) // Toggle switch sortujemy w obu stronach malejąco i rosnąco
                    {
                        if (currentTasks[j].Deadline > currentTasks[j + 1].Deadline)
                        {
                            TodoTask temp = currentTasks[j];
                            currentTasks[j] = currentTasks[j + 1];
                            currentTasks[j + 1] = temp;
                        }
                    }
                    else
                    {
                        if (currentTasks[j].Deadline < currentTasks[j + 1].Deadline)
                        {
                            TodoTask temp = currentTasks[j];
                            currentTasks[j] = currentTasks[j + 1];
                            currentTasks[j + 1] = temp;
                        }
                    }

                }

                //if (!swapped) //nie sortuje do koca temu odzrucim ta ideja(bedzie nie optm bubble)
                //    break;
            }
            //RefreshDataGrid();
            PerformSearch(); // meniam bo refresh data grid nie uwzglednia tagi i poszukiwanie
        }

        // - ALGORYTM BĄBELKOWY (Bubble Sort) - PO Statusie -
        private void btnSortStatus_Click(object sender, EventArgs e)
        {
            int n = currentTasks.Count;
            directionStatus = !directionStatus; // False - rosnąco True malejąco Toggle switch 
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (directionStatus == false)
                    {
                        if (currentTasks[j].Status > currentTasks[j + 1].Status)
                        {
                            TodoTask temp = currentTasks[j];
                            currentTasks[j] = currentTasks[j + 1];
                            currentTasks[j + 1] = temp;
                        }
                    }
                    else
                    {
                        if (currentTasks[j].Status < currentTasks[j + 1].Status)
                        {
                            TodoTask temp = currentTasks[j];
                            currentTasks[j] = currentTasks[j + 1];
                            currentTasks[j + 1] = temp;
                        }
                    }

                }
            }
            //RefreshDataGrid();
            PerformSearch(); // meniam bo refresh data grid nie uwzglednia tagi i poszukiwanie
        }

        // --- Metody ---

        // -- Metoda Perfom Search --
        private void PerformSearch()
        {

            if (currentTasks == null) return; // Jezeli jest pusta

            string query = txtSearch.Text.Trim().ToLower(); // Pobieramy tekst z paska wyszukiwania i zamieniamy na małe litery
            string selectedTag = cmbFilterTags.SelectedItem?.ToString(); // Pobieramy wybrany tag z ComboBoxa
            string selectedStatus = cmbFilterStatus.SelectedItem?.ToString(); // Pobieramy wybrany status z ComboBoxa
            string selectedPriority = cmbFilterPriority.SelectedItem?.ToString(); // Pobieramy wybrany priorytet z ComboBoxa

            List<TodoTask> filteredList = new List<TodoTask>();

            foreach (var task in currentTasks)
            {

                string safeTitle = (task.Title ?? "").ToLower();
                string safeDesc = (task.Description ?? "").ToLower();
                string dateString = task.Deadline.ToString("yyyy-MM-dd HH:mm");

                // --- WARUNEK TEKST ---
                bool matchText = string.IsNullOrEmpty(query) || 
                                 safeTitle.Contains(query) || // Szukamy w tytule
                                 safeDesc.Contains(query) || // Szukamy w opisie
                                 dateString.Contains(query); // Szukamy w dacie

                // --- WARUNEK TAG ---
                bool matchTag = true;
                if (selectedTag == "Bez tagów")
                {
                    matchTag = (task.Tags.Count == 0);
                }
                else if (selectedTag != "Wszystkie" && !string.IsNullOrEmpty(selectedTag))
                {
                    matchTag = task.Tags.Contains(selectedTag);
                }

                // --- WARUNEK STATUS ---
                bool matchStatus = true;
                if (selectedStatus != "Wszystkie" && !string.IsNullOrEmpty(selectedStatus))
                {
                    matchStatus = (task.Status.ToString() == selectedStatus);
                }

                // --- WARUNEK D: PRIORYTET ---
                bool matchPriority = true;
                if (selectedPriority != "Wszystkie" && !string.IsNullOrEmpty(selectedPriority))
                {
                    matchPriority = (task.Priority.ToString() == selectedPriority);
                }

                // --- DODANIE TYLKO MATCHED DATA ---
                if (matchText && matchTag && matchStatus && matchPriority)
                {
                    filteredList.Add(task);
                }
            }

            RefreshDataGrid(filteredList);
        }


        // --  Metoda logiki Expired Zadań --
        private void CheckForExpiredTasks()
        {
            bool anyChange = false; // Flaga zeby wiedziec czy musimy zapisywac plik

            foreach (var task in currentTasks)
            {
                // Sprawdzamy tylko zadania, które NIE są zakończone
                if (task.Status != TaskStatus.Completed)
                {
                    // Jeśli termin jest mniejszy (wcześniejszy) niż TERAZ
                    if (task.Deadline < DateTime.Now)
                    {
                        // Jeśli status nie jest jeszcze Expired, to go zmieniamy
                        if (task.Status != TaskStatus.Expired)
                        {
                            task.Status = TaskStatus.Expired;
                            anyChange = true;
                        }
                    }
                }
            }

            // Jesli cos zmenilo sie (jakies zadanie "Expired") zapisujemy to do pliku
            if (anyChange)
            {
                taskService.SaveTasks(currentTasks);
            }
        }


        // -- TAGOWANIE --

        // -- Metoda do aktualizacji listy tagów w filtrze --
        private void UpdateTagFilterList() // Aktualizacja listy tagów w ComboBox
        {
            // 1. Zapamiętujemy aktualne zaznaczenie inaczej zniknie po odświeżeniu na "wszystkie"
            string currentSelection = cmbFilterTags.SelectedItem?.ToString();

            // HashSet aby tagi nie powtarzaly sie (HashSet automatycznie ignoruje duplikaty.)
            HashSet<string> uniqueTags = new HashSet<string>();

            foreach (var task in currentTasks)
            {
                foreach (var tag in task.Tags)
                {
                    // W toerri mozno dodac to .lower aby UWB i uwb byli traktowani jako ten sam tag 
                    uniqueTags.Add(tag);
                }
            }

            // Sortujemy tagi po alfabetu
            List<string> sortedTags = uniqueTags.OrderBy(t => t).ToList(); // Order by oczekuje funkcja dla sortowania sortujaca (t => t) czyli po prostu sam tag

            // - Clear i wypełniamy ComboBox -
            cmbFilterTags.Items.Clear(); // Czyścimy najpierw aby nie było duplikatów

            // Opcje stałe
            cmbFilterTags.Items.Add("Wszystkie"); 
            cmbFilterTags.Items.Add("Bez tagów");

            // Opcje dynamiczne czyli tagi zadań
            foreach (var tag in sortedTags)
            {
                cmbFilterTags.Items.Add(tag);
            }

            
            if (currentSelection != null && cmbFilterTags.Items.Contains(currentSelection)) // jezeli user usunal wszytkie zadanie z danym tagiem to nie bedzie on istnial w liscie
            {
                cmbFilterTags.SelectedItem = currentSelection;
            }
            else // jeżeli nie ma takiego tagu to ustawiamy na "Wszystkie"
            {
                cmbFilterTags.SelectedIndex = 0; // Wybierz "Wszystkie"
            }
        }

        // -- Metody obsługi zmiany filtrów --
        private void cmbFilterTags_SelectedIndexChanged(object sender, EventArgs e) // Wybranie z listy tagów
        {
            PerformSearch(); // Uruchom filtrowanie od razu po wybraniu opcji
        }

        // -- Metody obsługi zmiany filtrów --
        private void cmbFilterStatus_SelectedIndexChanged(object sender, EventArgs e) // Wybranie z listy statusów
        {
            PerformSearch(); // Uruchom filtrowanie od razu po wybraniu opcji
        }

        // -- Metody obsługi zmiany filtrów --
        private void cmbFilterPriority_SelectedIndexChanged(object sender, EventArgs e) // Wybranie z listy priorytetów
        {
            PerformSearch();
        }

        // ---- [Panel Statystyki] ----

        // --- Metoda aktualizacji statystyk ---
        private void UpdateStatistics()
        {
            
            int total = currentTasks.Count;
            int completed = 0; // Wypelnione zadania (Completed)
            int expired = 0; // Expired zadania 
            int pending = 0; // Do zrobienia (NotCompleted)

            foreach (var t in currentTasks)
            {
                if (t.Status == TaskStatus.Completed) 
                {
                    completed++;
                }
                else if (t.Status == TaskStatus.Expired)
                {
                    expired++;
                }
                else 
                {
                    pending++; 
                }
            }

            // Aktualizacja napisów
            lblTotal.Text = "Wszystkie zadania: " + total;
            lblCompleted.Text = "Zakończone: " + completed;
            lblExpired.Text = "Przeterminowane: " + expired;
            lblPending.Text = "Do zrobienia: " + pending;

            // Pasek loading (liczymy tylko Completed vs wszystkie)
            if (total > 0) // Unikamy dzielenia przez zero
            {
                int percent = (int)((double)completed / total * 100); // Dzielimy liczbę zakończonych przez wszystkie.
                progressBarTasks.Value = percent;
                lblProgressPercent.Text = percent + "%";
            }
            else
            {
                progressBarTasks.Value = 0;
                lblProgressPercent.Text = "0%";
            }

            // --- Aktualizacja wykresu słupkowego ---

            // 1. Czyścimy stare dane
            chartStats.Series.Clear();
            chartStats.Legends.Clear(); 
            chartStats.Legends.Add("Legenda");

            // Zapelniamy chart dannymi
            Series series = new Series
            {
                Name = "Zadania",
                Color = Color.Blue,
                IsVisibleInLegend = false, // Bo sa kolory
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Column, // Słupkowy pionowy (Column)
                LabelBackColor = Color.White,  // Białe tło pod cyferką
                LabelForeColor = Color.Black,  // Czarny kolor cyferki (dla kontrastu)
                // Ramka dla cyferki
                LabelBorderColor = Color.Black,
                LabelBorderWidth = 1
            };

            chartStats.Series.Add(series);

            // -- Punkty danych  --
            // AddXY("Podpis", Wartość)

            // Słupek 1: Completed (Zielony)
            int p1 = series.Points.AddXY("Zakończone", completed);
            series.Points[p1].Color = Color.LightGreen;
            series.Points[p1].Label = completed.ToString(); // Liczba na slupku

            // Słupek 2: Not Completed (Niebieski)
            int p2 = series.Points.AddXY("Do zrobienia", pending);
            series.Points[p2].Color = Color.LightSkyBlue;
            series.Points[p2].Label = pending.ToString(); // Liczba na slupku

            // Słupek 3: Expired (Czerwony)
            int p3 = series.Points.AddXY("Przeterminowane", expired);
            series.Points[p3].Color = Color.Salmon;
            series.Points[p3].Label = expired.ToString();

            // Odświeżamy wykres
            chartStats.Invalidate();


        }


        // ---- [Panel About] ----

        // --- Przyciski ---

        // -- Otwieranie dokumentacji --
        private void btnOpenDoc_Click(object sender, EventArgs e)
        {
            string fileName = "Dokumentacja Menedżer zadań (To-Do) z priorytetami.docx";

            
            string appPath = Application.StartupPath; // Path do folderu aplikacji

            
            string fullPath = Path.Combine(appPath, fileName); // Pełna ścieżka do pliku dokumentacji [C:\Projekty\ToDo\bin\Debug\Dokumentacja Menedżer zadań (To-Do) z priorytetami.docx]

            try
            {
                if (File.Exists(fullPath)) // Check czy plik istnieje
                {
                    // Otwieramy plik w domyślnym programie (np. Acrobat Reader lub Word)
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = fullPath, // Co otwieramy
                        UseShellExecute = true // Ważne dla .NET Core/.NET 5+, żeby otworzyć dokument ( Jezli uzytkownik niema word to otworzy w domyslnej systemu )
                    };
                    Process.Start(psi);
                }
                else
                {
                    MessageBox.Show($"Nie znaleziono pliku dokumentacji!\nSzukano w: {fullPath}",
                                    "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas otwierania dokumentacji:\n{ex.Message}",
                                "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        // ---- [Panel Ustawienia] ----

        // --- Radio przyciski ---
        private void rbLight_CheckedChanged(object sender, EventArgs e)
        {
            if (rbLight.Checked)
            {
                ApplyTheme(false); // Włącz jasny
            }
        }

        private void rbDark_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDark.Checked)
            {
                ApplyTheme(true); // Włącz ciemny
            }
        }
        
        // --- Metody ---

        // -- Metoda zmiana koloru tła aplikacji --
        private void ApplyTheme(bool isDark)
        {
            // Definiujemy kolory dla obu motywów
            Color mainBgColor = isDark ? Color.FromArgb(45, 45, 45) : Color.White; // Tło główne (Ciemnoszare vs Białe)
            Color contentColor = isDark ? Color.FromArgb(45, 45, 48) : Color.White; // Tło paneli
            Color textColor = isDark ? Color.White : Color.Black; // Tekst

            // Forma główna i panel zawartości
            this.BackColor = mainBgColor;
            panelContent.BackColor = mainBgColor;

            // Panele 
            panelStatistics.BackColor = contentColor;
            panelSettings.BackColor = contentColor;
            panelAbout.BackColor = contentColor;

            // Zmiana kolorów w Tabeli (DataGridView)
            dgvTasks.BackgroundColor = contentColor;
            dgvTasks.DefaultCellStyle.BackColor = contentColor;
            dgvTasks.DefaultCellStyle.ForeColor = textColor;
            dgvTasks.ColumnHeadersDefaultCellStyle.BackColor = isDark ? Color.Maroon : Color.DarkRed; // Nieco inny odcień
            dgvTasks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTasks.EnableHeadersVisualStyles = false; // Aby kolory nagłówków działały poprawnie

            // Zmiana kolorów napisów (Labels)


            ChangeLabelsColorInPanel(panelStatistics, textColor); // 
            ChangeLabelsColorInPanel(panelSettings, textColor);
            ChangeLabelsColorInPanel(panelAbout, textColor);

            // Label w GroupBox
            groupBox1.ForeColor = textColor; // GroupBox w Settings
            rbDark.ForeColor = textColor;
            rbLight.ForeColor = textColor;

            // Inne kontrolki
            txtSearch.BackColor = isDark ? Color.FromArgb(60, 60, 60) : Color.White;
            txtSearch.ForeColor = textColor;
        }

        // -- Metoda pomocnicza do zmiany kolorów Labeli w danym panelu --
        private void ChangeLabelsColorInPanel(Panel p, Color c)
        {
            foreach (Control ctrl in p.Controls) // Przechodzimy przez wszystkie kontrolki w panelu
            {
                if (ctrl is Label) // Jeśli kontrolka jest typu Label
                {
                    ctrl.ForeColor = c; // Zmieniamy kolor tekstu
                }
            }
        }

        private void panelAbout_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
