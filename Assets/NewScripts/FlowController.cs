using System;

namespace NewScripts
{
    public class FlowController
    {
        public int Year { get; set; } = 1;
        public int Month { get; set; } = 1;
        public int Day { get; set; } = 1;

        public void IncrementDay()
        {
            if (Day == 21)
            {
                Day = 1;
            }
            else
            {
                Day++;
            }
        }

        public void IncrementMonth()
        {
            if (Month == 12)
            {
                Year++;
                Month = 1;
            }
            else
            {
                Month++;
            }
        }

        public void Reset()
        {
            Year = 1;
            Month = 1;
            Day = 1;
        }

        public string Current()
        {
            return $"{Day}.{Month}.{Year}";
        }
        
        public void StartMonth()
        {
            // Unternehmen:
            // Lohnsatz anpassen
            // Verkaufspreis anpassen
            // Mitarbeiterzahl anpassen (offene Stellen)
            
            // Privathaushalte:
            // Arbeitsstelle suchen (wenn arbeitslos)
            // Arbeitsstelle wechseln (wenn beschäftigt)
            // jeweils beste Bezahlung
            // billigere Anbieter suchen
            // Budget anpassen (Anteil an Einkommen)
        }

        public void StartDay()
        {
            // PH:
            // Kauf Güter
            
            // UN:
            // Produktion Konsumgüter, nach Anzahl Bschäftigte
        }

        public void EndMonth()
        {
            // UN
            // Gewinne verteilen??
            // MA bezahlen
            // Entlassung
        }
    }
}