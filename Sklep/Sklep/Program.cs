using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sklep
{
    class Program
    {
        class Uzytkownik
        {
            private static uint liczba_uzytkownikow = 0;
            public uint id { get; }
            public string email { get; protected set; }
            public string haslo { get; protected set; }

            public Uzytkownik(string wiersz) {
                string[] wiersz_tab = wiersz.Split(';');
                id = uint.Parse(wiersz_tab[0]);
                email = wiersz_tab[1];
                haslo = wiersz_tab[2];

                liczba_uzytkownikow++;
            }

            public Uzytkownik(string email, string haslo)
            {
                id = liczba_uzytkownikow;
                liczba_uzytkownikow++;
                this.email = email;
                this.haslo = haslo;
            }
        }

        class Klient : Uzytkownik
        {
            public Klient(string wiersz) : base(wiersz) { }
            public Klient(string email, string haslo) : base(email, haslo) { }

            //jakies tam metody zwiazane z zakupami
        }

        class Administrator : Uzytkownik
        {
            public Administrator(string wiersz) : base(wiersz) { }

            //jakies tam metody zwiazane z zarzadzaniem sklepem
        }

        class Lista_uzytkownikow
        {
            private List<Uzytkownik> uzytkownicy;

            public Lista_uzytkownikow(string sciezka)
            {
                uzytkownicy = new List<Uzytkownik>();

                if (!File.Exists(sciezka))
                {
                    return;
                }

                StreamReader streamReader = new StreamReader(sciezka);
                string wiersz;

                while((wiersz = streamReader.ReadLine()) != null)
                {
                    if (wiersz[0] == '0')
                    {
                        uzytkownicy.Add(new Administrator(wiersz));
                    }
                    else
                    {
                        uzytkownicy.Add(new Klient(wiersz));
                    }
                }
            }

            public void dodaj_uzytkownika(Uzytkownik nowy_uzytkownik)
            {
                uzytkownicy.Add(nowy_uzytkownik);
            }

            public Uzytkownik? proba_zalogowania(string email, string haslo)
            {
                if(email is null || haslo is null)
                {
                    return null;
                }
                foreach(Uzytkownik uzytkownik in uzytkownicy)
                {
                    if(email == uzytkownik.email && haslo == uzytkownik.haslo)
                    {
                        return uzytkownik;
                    }
                }
                return null;
            }

            public bool email_wystepuje(string email)
            {
                foreach (Uzytkownik uzytkownik in uzytkownicy)
                {
                    if (email == uzytkownik.email)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        class Produkt
        {
            public uint id { get; }
            public string nazwa { get; private set; }
            public uint liczba_sztuk { get; private set; }
            public double cena { get; private set; }

            public Produkt(string nazwa, uint liczba_sztuk, double cena)
            {
                this.nazwa = nazwa;
                this.liczba_sztuk = liczba_sztuk;
            }

            public void zwieksz_liczbe_sztuk(uint o_ile)
            {
                liczba_sztuk += o_ile;
            }

            public bool zmniejsz_liczbe_sztuk(uint o_ile)
            {
                if(liczba_sztuk - o_ile >= 0)
                {
                    liczba_sztuk -= o_ile;
                    return true;
                }
                return false;
            }
        }

        class Magazyn
        {
            private List<Produkt> produkty;

            public Magazyn()
            {
                produkty = new List<Produkt>();
            }

            public Produkt? znajdz_po_id(uint id)
            {
                foreach(Produkt produkt in produkty)
                {
                    if(produkt.id == id)
                    {
                        return produkt;
                    }
                }
                return null;
            }
        }

        class Interfejs
        {
            static public Uzytkownik start(Lista_uzytkownikow uzytkownicy)
            {
                Console.WriteLine("SKLEP");
                string akcja;
                while (true)
                {
                    Console.WriteLine("Wpisz 1, aby się zalogować. Wpisz 2, aby utworzyć konto.");
                    akcja = Console.ReadLine();

                    if(akcja == "1")
                    {
                        return logowanie(uzytkownicy);
                    }
                    else if(akcja == "2")
                    {
                        return rejestracja(uzytkownicy);
                    }
                }
            }

            static public Uzytkownik logowanie(Lista_uzytkownikow uzytkownicy)
            {
                string email, haslo;
                Uzytkownik uzytkownik;

                while (true)
                {
                    Console.WriteLine("LOGOWANIE");
                    Console.Write("Email: ");
                    email = Console.ReadLine();
                    Console.Write("Hasło: ");
                    haslo = Console.ReadLine();

                    uzytkownik = uzytkownicy.proba_zalogowania(email, haslo);

                    if (uzytkownik is not null)
                    {
                        Console.WriteLine("Zalogowano.");
                        return uzytkownik;
                    }
                    else
                    {
                        Console.WriteLine("Niepoprawne dane logowania.");
                    }
                }
            }

            static public Uzytkownik rejestracja(Lista_uzytkownikow uzytkownicy_zapisani)
            {
                string email, haslo;
                Uzytkownik nowy_uzytkownik;

                Console.WriteLine("REJESTRACJA");

                while (true)
                {
                    Console.Write("Email: ");
                    email = Console.ReadLine();
                    if (email == "")
                    {
                        Console.WriteLine("Adres email nie może być pusty. Wpisz poprawny adres email.");
                        continue;
                    }
                    else if (uzytkownicy_zapisani.email_wystepuje(email))
                    {
                        Console.WriteLine("Istnieje już konto z podanym adresem email. Podaj inny adres email.");
                        continue;
                    }
                    break;
                }

                Console.Write("Hasło: ");
                haslo = Console.ReadLine();

                nowy_uzytkownik = new Uzytkownik(email, haslo);
                uzytkownicy_zapisani.dodaj_uzytkownika(nowy_uzytkownik);
                Console.WriteLine("Konto zostało utworzone.");
                return nowy_uzytkownik;
            }
        }

        static void Main(string[] args)
        {
            const string sciezka_uzytkownicy = "../../../../dane/uzytkownicy.csv";
            const string sciezka_koszyki = "../../../../dane/koszyk"; //+ id + ".csv"
            Lista_uzytkownikow uzytkownicy = new Lista_uzytkownikow(sciezka_uzytkownicy);
            Uzytkownik zalogowany_uzytkownik;

            zalogowany_uzytkownik = Interfejs.start(uzytkownicy);









            Console.ReadKey();
        }
    }
}