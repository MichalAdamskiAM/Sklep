using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sklep
{
    class Program
    {
        const string odstep = "\n\n";
        const string sciezka_uzytkownicy = "../../../../dane/uzytkownicy.csv";
        const string sciezka_koszyki_cz1 = "../../../../dane/koszyk";
        const string sciezka_koszyki_cz3 = ".csv";
        const string sciezka_magazyn = "../../../../dane/magazyn.csv";

        abstract class Uzytkownik
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

            public string format_do_pliku()
            {
                return id + ";" + email + ";" + haslo;
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

                StreamReader streamReader = new StreamReader(sciezka, System.Text.Encoding.UTF8);
                string wiersz;

                while((wiersz = streamReader.ReadLine()) is not null)
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
                streamReader.Close();
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

            public void zapisz_do_pliku(string sciezka)
            {
                using (StreamWriter streamWriter = new StreamWriter(sciezka))
                {
                    foreach (Uzytkownik uzytkownik in uzytkownicy)
                    {
                        streamWriter.WriteLine(uzytkownik.format_do_pliku());
                    }
                }
            }
        }

        class Produkt
        {
            public uint id { get; }
            public string nazwa { get; private set; }
            public uint liczba_sztuk { get; private set; }
            public double cena { get; private set; }

            public Produkt(uint id, string nazwa, uint liczba_sztuk, double cena)
            {
                this.id = id;
                this.nazwa = nazwa;
                this.liczba_sztuk = liczba_sztuk;
                this.cena = cena;
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

            public string format_do_pliku_magazyn()
            {
                return id + ";" + nazwa + ";" + liczba_sztuk + ";" + cena;
            }

            public string format_do_pliku_koszyk()
            {
                return id + ";" + liczba_sztuk;
            }
        }

        abstract class Zbior_produktow
        {
            protected List<Produkt> produkty;

            public Zbior_produktow()
            {
                produkty = new List<Produkt>();
            }

            public Produkt? znajdz_po_id(uint id)
            {
                foreach (Produkt produkt in produkty)
                {
                    if (produkt.id == id)
                    {
                        return produkt;
                    }
                }
                return null;
            }

            public void wyswietl()
            {
                int i = 1;
                foreach (Produkt produkt in produkty)
                {
                    if (produkt.liczba_sztuk > 0)
                    {
                        Console.WriteLine(i++ + ". " + produkt.nazwa + " - " + produkt.cena + " zł. Liczba sztuk: " + produkt.liczba_sztuk + ".");
                    }
                }
            }

            abstract public void zapisz_do_pliku(string sciezka);
        }

        class Magazyn : Zbior_produktow
        {
            private uint nastepne_wolne_id = 0;

            public Magazyn(string sciezka)
            {
                produkty = new List<Produkt>();

                if (!File.Exists(sciezka))
                {
                    return;
                }

                StreamReader streamReader = new StreamReader(sciezka, System.Text.Encoding.UTF8);
                string wiersz, nazwa;
                string[] wiersz_tab;
                uint id = 0, liczba_sztuk;
                double cena;

                while ((wiersz = streamReader.ReadLine()) is not null)
                {
                    wiersz_tab = wiersz.Split(';');
                    id = uint.Parse(wiersz_tab[0]);
                    nazwa = wiersz_tab[1];
                    liczba_sztuk = uint.Parse(wiersz_tab[2]);
                    cena = double.Parse(wiersz_tab[3], System.Globalization.CultureInfo.InvariantCulture); //InvariantCulture bo bez tego postrzega program jako polski wiec oczekuje przecinka jako separatora dziesietnego

                    produkty.Add(new Produkt(id, nazwa, liczba_sztuk, cena));
                }
                streamReader.Close();
                nastepne_wolne_id = id + 1;
            }

            public void dodaj_produkt(string nazwa, uint liczba_sztuk, double cena)
            {
                produkty.Add(new Produkt(nastepne_wolne_id++, nazwa, liczba_sztuk, cena));
            }

            public void wykupiono(uint id, uint liczba_sztuk)
            {
                znajdz_po_id(id)?.zmniejsz_liczbe_sztuk(liczba_sztuk);
            }

            public override void zapisz_do_pliku(string sciezka)
            {
                using (StreamWriter streamWriter = new StreamWriter(sciezka))
                {
                    foreach (Produkt produkt in produkty)
                    {
                        streamWriter.WriteLine(produkt.format_do_pliku_magazyn());
                    }
                }
            }
        }

        class Koszyk : Zbior_produktow
        {
            public uint liczba_produktow_utraconych { get; protected set; } = 0;

            public Koszyk(string sciezka, Magazyn magazyn)
            {
                produkty = new List<Produkt>();

                if (!File.Exists(sciezka))
                {
                    return;
                }

                StreamReader streamReader = new StreamReader(sciezka, System.Text.Encoding.UTF8);
                string wiersz, nazwa;
                string[] wiersz_tab;
                uint id, liczba_sztuk;
                double cena;
                Produkt? rodzaj_wczytywanego_produktu;

                while ((wiersz = streamReader.ReadLine()) is not null)
                {
                    wiersz_tab = wiersz.Split(';');
                    id = uint.Parse(wiersz_tab[0]);
                    liczba_sztuk = uint.Parse(wiersz_tab[1]);

                    rodzaj_wczytywanego_produktu = magazyn.znajdz_po_id(id);
                    if (rodzaj_wczytywanego_produktu is null)
                    {
                        liczba_produktow_utraconych++;
                        break;
                    }

                    nazwa = rodzaj_wczytywanego_produktu.nazwa;
                    cena = rodzaj_wczytywanego_produktu.cena;

                    produkty.Add(new Produkt(id, nazwa, liczba_sztuk, cena));
                }
                streamReader.Close();
            }

            public double kwota()
            {
                double kwota = 0;
                foreach(Produkt produkt in produkty)
                {
                    kwota += produkt.cena;
                }
                return kwota;
            }

            public override void zapisz_do_pliku(string sciezka)
            {
                using (StreamWriter streamWriter = new StreamWriter(sciezka))
                {
                    foreach (Produkt produkt in produkty)
                    {
                        streamWriter.WriteLine(produkt.format_do_pliku_koszyk());
                    }
                }
            }
        }

        static class Interfejs
        {
            static public Uzytkownik start(Lista_uzytkownikow uzytkownicy)
            {
                Console.WriteLine("APLIKACJA SKLEP - Michał Adamski i Bartosz Kurto 4P1T");
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
                Uzytkownik? uzytkownik;

                while (true)
                {
                    Console.WriteLine(odstep);
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

                Console.WriteLine(odstep);
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

                nowy_uzytkownik = new Klient(email, haslo);
                uzytkownicy_zapisani.dodaj_uzytkownika(nowy_uzytkownik);
                Console.WriteLine("Konto zostało utworzone.");
                return nowy_uzytkownik;
            }

            static public Koszyk? panel_glowny(Uzytkownik zalogowany_uzytkownik, Magazyn magazyn)
            {
                if(zalogowany_uzytkownik.id == 0)
                {
                    panel_administratora(magazyn);
                    return null;
                }
                else
                {
                    Koszyk koszyk = new Koszyk(sciezka_koszyki_cz1 + zalogowany_uzytkownik.id + sciezka_koszyki_cz3, magazyn);
                    panel_klienta(zalogowany_uzytkownik, magazyn, koszyk);
                    return koszyk;
                }
            }

            static public void panel_administratora(Magazyn magazyn)
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - PANEL GŁÓWNY ADMINISTRATORA");
                string akcja;
                while (true)
                {
                    Console.WriteLine("Wybierz akcję:\n 1. Wyświetl produkty dostępne w sklepie.\n 2. Zapisz dane i zamknij aplikację.");
                    akcja = Console.ReadLine();
                    if(akcja == "1")
                    {
                        administrator_zarzadzanie_magazynem(magazyn);
                        return;
                    }
                    else if(akcja == "2")
                    {
                        return;
                    }
                    Console.WriteLine("Wprowadzono niepoprawne polecenie.");
                }
            }

            static public void panel_klienta(Uzytkownik zalogowany_klient, Magazyn magazyn, Koszyk koszyk)
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - PANEL GŁÓWNY KLIENTA");
                string akcja;
                while (true)
                {
                    Console.WriteLine("Wybierz akcję:\n 1. Wyświetl produkty dostępne w sklepie.\n 2. Wyświetl swój koszyk\n 3. Zapisz dane i zamknij aplikację.");
                    akcja = Console.ReadLine();
                    if (akcja == "1")
                    {
                        klient_dostepne_produkty(magazyn);
                        return;
                    }
                    else if (akcja == "2")
                    {
                        klient_koszyk(koszyk);
                        return;
                    }
                    else if (akcja == "3")
                    {
                        return;
                    }
                    Console.WriteLine("Wprowadzono niepoprawne polecenie.");
                }
            }

            static public void klient_dostepne_produkty(Magazyn magazyn)
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - DOSTĘPNE PRODUKTY");
                Console.WriteLine("Produkty aktualnie dostępne w sklepie:");
                magazyn.wyswietl();
                Console.WriteLine("Wpisz numer produktu, aby dodać go do koszyka. Kliknij enter, aby wrócić do panelu głównego.");
                //todo
            }

            static public void klient_koszyk(Koszyk koszyk)
            {
                Console.WriteLine(odstep);
                Console.WriteLine("TWÓJ KOSZYK");
                Console.WriteLine("Produkty znajdujące się w twoim koszyku:");
                koszyk.wyswietl();
                Console.WriteLine("Wpisz numer produktu, aby dodać go do koszyka. Wpisz b, aby dokonać zakupu całej zawartości koszyka. Kliknij enter, aby wrócić do panelu głównego.");
                //todo
            }

            static public void administrator_zarzadzanie_magazynem(Magazyn magazyn)
            {
                magazyn.wyswietl();
                Console.WriteLine("Wpisz numer pozycji, aby edytować produkt. Kliknij enter, aby wrócić do panelu głównego.");
                //todo
            }
        }

        static void Main(string[] args)
        {
            Lista_uzytkownikow uzytkownicy = new Lista_uzytkownikow(sciezka_uzytkownicy);
            Uzytkownik zalogowany_uzytkownik;

            zalogowany_uzytkownik = Interfejs.start(uzytkownicy);
            Magazyn magazyn = new Magazyn(sciezka_magazyn);
            Koszyk? koszyk = Interfejs.panel_glowny(zalogowany_uzytkownik, magazyn);

            uzytkownicy?.zapisz_do_pliku(sciezka_uzytkownicy);
            magazyn?.zapisz_do_pliku(sciezka_magazyn);
            koszyk?.zapisz_do_pliku(sciezka_koszyki_cz1 + zalogowany_uzytkownik.id + sciezka_koszyki_cz3);
        }
    }
}