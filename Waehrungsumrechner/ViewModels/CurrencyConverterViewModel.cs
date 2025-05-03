using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Waehrungsumrechner.Models;
using Waehrungsumrechner.Services;

namespace Waehrungsumrechner.ViewModels
{
    /// <summary>
    /// ViewModel für den Währungsumrechner nach dem MVVM-Pattern.
    /// Kapselt alle Daten, lädt sie aus dem Service und sorgt für automatische Aktualisierung der UI.
    /// </summary>
    public class CurrencyConverterViewModel : INotifyPropertyChanged
    {
        // --- Felder und Services ---

        // Unser Service-Interface, das die API-Aufrufe kapselt
        private readonly ICurrencyService _service;

        // Intern gehaltene Werte für Quell- und Zielwährung
        private Currency _sourceCurrency;
        private Currency _targetCurrency;

        // Der Betrag, den der Nutzer eingibt (default 1)
        private decimal _amount = 1m;

        // Das berechnete Ergebnis nach der Umrechnung
        private decimal _converted;

        // Zwischengespeicherte Umrechnungskurse für die aktuell gewählte Basis-Währung
        private Dictionary<string, decimal> _currentRates = new Dictionary<string, decimal>();

        // --- Properties für die Bindings ---

        /// <summary>
        /// Liste aller verfügbaren Währungen (Code + Name).
        /// ObservableCollection benachrichtigt die UI automatisch bei Änderungen (Add/Remove).
        /// </summary>
        public ObservableCollection<Currency> Currencies { get; }
            = new ObservableCollection<Currency>();

        /// <summary>
        /// Parameterloser Konstruktor für XAML.
        /// Erzeugt hier einfach einen neuen CurrencyService.
        /// </summary>
        public CurrencyConverterViewModel()
            : this(new CurrencyService())
        { }

        /// <summary>
        /// Konstruktor mit expliziter Service-Injektion (z.B. für Unit-Tests).
        /// Startet im Anschluss das asynchrone Laden der Währungen.
        /// </summary>
        public CurrencyConverterViewModel(ICurrencyService service)
        {
            _service = service;
            // Fire-and-forget: Lädt die Währungen im Hintergrund
            _ = LoadCurrenciesAsync();
        }

        /// <summary>
        /// Quellwährung, aus der umgerechnet werden soll.
        /// Setter lädt bei Änderung automatisch die neuen Kurse.
        /// </summary>
        public Currency SourceCurrency
        {
            get => _sourceCurrency;
            set
            {
                if (_sourceCurrency != value)
                {
                    _sourceCurrency = value;
                    OnPropertyChanged();            // benachrichtige UI
                    _ = LoadRatesAsync();           // lade neue Kurse asynchron
                }
            }
        }

        /// <summary>
        /// Zielwährung, in die umgerechnet werden soll.
        /// Setter löst sofort eine Neuberechnung aus.
        /// </summary>
        public Currency TargetCurrency
        {
            get => _targetCurrency;
            set
            {
                if (_targetCurrency != value)
                {
                    _targetCurrency = value;
                    OnPropertyChanged();            // benachrichtige UI
                    Recalculate();                  // berechne das Ergebnis neu
                }
            }
        }

        /// <summary>
        /// Betrag zur Umrechnung. Jede Änderung löst eine Neuberechnung aus.
        /// </summary>
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    OnPropertyChanged();            // benachrichtige UI
                    Recalculate();                  // berechne das Ergebnis neu
                }
            }
        }

        /// <summary>
        /// Ergebnis der Umrechnung. Wird nur innerhalb des ViewModels gesetzt.
        /// </summary>
        public decimal Converted
        {
            get => _converted;
            private set
            {
                if (_converted != value)
                {
                    _converted = value;
                    OnPropertyChanged();            // benachrichtige UI
                }
            }
        }

        // --- Methoden zur Datenbeschaffung und Berechnung ---

        /// <summary>
        /// Lädt die Liste aller Währungen aus dem Service und
        /// setzt Standardwerte (EUR → USD, Betrag 1).
        /// </summary>
        private async Task LoadCurrenciesAsync()
        {
            // Rufe die Währungsliste ab
            var list = await _service.GetCurrenciesAsync();

            // Fülle die ObservableCollection (UI wird automatisch aktualisiert)
            foreach (var c in list)
                Currencies.Add(c);

            // Initialisiere Standard-Auswahl (ERP → USD)
            SourceCurrency = Currencies.FirstOrDefault(c => c.Code == "EUR");
            TargetCurrency = Currencies.FirstOrDefault(c => c.Code == "USD");

            // Setze den Eingabebetrag auf 1, damit direkt ein Ergebnis sichtbar ist
            Amount = 1m;
        }

        /// <summary>
        /// Lädt die Umrechnungskurse für die aktuell gewählte Basis-Währung.
        /// Ruft anschließend Recalculate auf, um das Ergebnis zu aktualisieren.
        /// </summary>
        private async Task LoadRatesAsync()
        {
            // Wenn keine Basis-Währung gesetzt ist, abbrechen
            if (SourceCurrency == null) return;

            // Kurse beim Service abfragen
            _currentRates = await _service.GetRatesAsync(SourceCurrency.Code);

            // Ergebnis mit den neuen Kursen neu berechnen
            Recalculate();
        }

        /// <summary>
        /// Führt die eigentliche Umrechnung durch:
        /// Converted = Amount * (Kurs von Basis → Ziel).
        /// Falls kein Kurs vorhanden ist, wird 0 gesetzt.
        /// </summary>
        private void Recalculate()
        {
            // Wenn keine Kurse oder keine Ziel-Währung vorhanden, Ergebnis auf 0 setzen
            if (_currentRates == null || TargetCurrency == null)
            {
                Converted = 0;
                return;
            }

            // Versuche, den Kurs für die Ziel-Währung aus dem Dictionary zu lesen
            if (_currentRates.TryGetValue(TargetCurrency.Code, out var rate))
                Converted = Amount * rate;    // korrekt den Betrag umrechnen
            else
                Converted = 0;               // falls kein Kurs da ist
        }

        // --- INotifyPropertyChanged Implementation ---

        /// <summary>
        /// Event, das die UI hört, um Property-Änderungen zu erkennen.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Methode, um das PropertyChanged-Event auszulösen.
        /// Der CallerMemberName-Attribute füllt automatisch den Property-Namen aus.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
