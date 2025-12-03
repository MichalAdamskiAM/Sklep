using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sklep
{
    class Program
    {
        static class Sciezki
        {
            public const string uzytkownicy = "../../../../dane/uzytkownicy.csv";
            public const string magazyn = "../../../../dane/magazyn.csv";
            public static string koszyk(uint id)
            {
                return "../../../../dane/koszyk" + id + ".csv";
            }
        }

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

            //jakies tam metody zwiazane z zakupami moze
        }

        class Administrator : Uzytkownik
        {
            public Administrator(string wiersz) : base(wiersz) { }

            //jakies tam metody zwiazane z zarzadzaniem sklepem moze
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

            public Produkt(Produkt kopiowany)
            {
                this.id = kopiowany.id;
                this.nazwa = kopiowany.nazwa;
                this.liczba_sztuk = 1;
                this.cena = kopiowany.cena;
            }

            public void zwieksz_liczbe_sztuk(uint o_ile = 1)
            {
                liczba_sztuk += o_ile;
            }

            public bool zmniejsz_liczbe_sztuk(uint o_ile = 1)
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

            public List<Produkt> wyswietl(string wyszukiwanie = "")
            {
                int i = 1;
                List<Produkt> wynik = new List<Produkt>();
                foreach (Produkt produkt in produkty)
                {
                    if (produkt.nazwa.Contains(wyszukiwanie) && produkt.liczba_sztuk > 0)
                    {
                        Console.WriteLine(" " + i++ + ". " + produkt.nazwa + " - " + produkt.cena + " zł. Liczba sztuk: " + produkt.liczba_sztuk + ".");
                        wynik.Add(produkt);
                    }
                }
                return wynik;
            }

            public void dodaj_produkt(Produkt produkt_do_dodania)
            {
                produkty.Add(produkt_do_dodania);
            }

            public void czysc()
            {
                produkty.Clear();
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
                    cena = double.Parse(wiersz_tab[3]);

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
            public uint liczba_rodzajow_utraconych { get; protected set; } = 0;

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
                        liczba_rodzajow_utraconych++;
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

        class Interfejs
        {
            private const string odstep = "\n\n";
            public Koszyk koszyk_zalogowanego { get; private set; }
            private Magazyn magazyn;
            public Uzytkownik zalogowany_uzytkownik { get; private set; }

            public Interfejs(Lista_uzytkownikow uzytkownicy, Magazyn magazyn)
            {
                this.magazyn = magazyn;
                Console.WriteLine("APLIKACJA SKLEP - Michał Adamski i Bartosz Kurto 4P1T\n");
                string akcja;
                while (zalogowany_uzytkownik is null)
                {
                    Console.WriteLine("Wpisz 1, aby się zalogować. Wpisz 2, aby utworzyć konto.");
                    akcja = Console.ReadLine();

                    if(akcja == "1")
                    {
                        logowanie(uzytkownicy);
                    }
                    else if(akcja == "2")
                    {
                        rejestracja(uzytkownicy);
                    }
                }
                koszyk_zalogowanego = new Koszyk(Sciezki.koszyk(zalogowany_uzytkownik.id), magazyn);
            }

            public void logowanie(Lista_uzytkownikow uzytkownicy)
            {
                string email, haslo;
                Uzytkownik? uzytkownik;

                while (true)
                {
                    Console.WriteLine(odstep);
                    Console.WriteLine("LOGOWANIE\n");
                    Console.Write("Email: ");
                    email = Console.ReadLine();
                    Console.Write("Hasło: ");
                    haslo = Console.ReadLine();

                    uzytkownik = uzytkownicy.proba_zalogowania(email, haslo);

                    if (uzytkownik is not null)
                    {
                        Console.WriteLine("Zalogowano.");
                        zalogowany_uzytkownik = uzytkownik;
                        return;
                    }
                    Console.WriteLine("Niepoprawne dane logowania.");
                }
            }

            public void rejestracja(Lista_uzytkownikow uzytkownicy_zapisani)
            {
                string email, haslo;
                Uzytkownik nowy_uzytkownik;

                Console.WriteLine(odstep);
                Console.WriteLine("REJESTRACJA\n");

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
                zalogowany_uzytkownik = nowy_uzytkownik;
            }

            public void panel_glowny()
            {
                if(zalogowany_uzytkownik.id == 0)
                {
                    panel_administratora();
                }
                else
                {
                    panel_klienta();
                }
            }

            public void panel_administratora()
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - PANEL GŁÓWNY ADMINISTRATORA\n");
                string akcja;
                while (true)
                {
                    Console.Write("Wybierz akcję:\n 1. Wyświetl produkty dostępne w sklepie.\n 2. Zapisz dane i zamknij aplikację.\nWpisz numer akcji: ");
                    akcja = Console.ReadLine();
                    if(akcja == "1")
                    {
                        administrator_zarzadzanie_magazynem();
                        return;
                    }
                    else if(akcja == "2")
                    {
                        return;
                    }
                    Console.WriteLine("Wprowadzono niepoprawne polecenie.");
                }
            }

            public void panel_klienta()
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - PANEL GŁÓWNY KLIENTA\n");
                string akcja;
                while (true)
                {
                    Console.Write("Wybierz akcję:\n 1. Wyświetl wszystkie produkty dostępne w sklepie.\n 2. Szukaj produktu.\n 3. Wyświetl swój koszyk/Dokonaj zakupu.\n 4. Zapisz dane i zamknij aplikację.\nWpisz numer akcji: ");
                    akcja = Console.ReadLine();
                    if (akcja == "1")
                    {
                        klient_dostepne_produkty();
                        return;
                    }
                    else if (akcja == "2")
                    {
                        Console.Write("Szukaj: ");
                        string wyszukiwanie = Console.ReadLine();
                        klient_dostepne_produkty(wyszukiwanie);
                        return;
                    }
                    else if (akcja == "3")
                    {
                        klient_koszyk();
                        return;
                    }
                    else if (akcja == "4")
                    {
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Polecenie nierozpoznane.");
                    }
                }
            }

            public void klient_dostepne_produkty(string wyszukiwanie = "")
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - DOSTĘPNE PRODUKTY\n");
                if(wyszukiwanie != "")
                {
                    Console.WriteLine("Produkty związane z " + wyszukiwanie + " dostępne aktualnie w sklepie:");
                }
                else
                {
                    Console.WriteLine("Produkty dostępne aktualnie w sklepie:");
                }
                List<Produkt> wyswietlone_produkty = magazyn.wyswietl(wyszukiwanie);

                while (true)
                {
                    Console.WriteLine("\nWpisz numer produktu, aby dodać go do koszyka. Wpisz x, aby wrócić do panelu głównego.");
                    string wprowadzony_ciag = Console.ReadLine();
                    if (uint.TryParse(wprowadzony_ciag, out uint numer_produktu) && --numer_produktu < wyswietlone_produkty.Count())
                    {
                        Produkt wybrany = wyswietlone_produkty[(int)numer_produktu];
                        Produkt? produkt_w_koszyku = koszyk_zalogowanego.znajdz_po_id(wybrany.id);

                        if (produkt_w_koszyku is not null)
                        {
                            produkt_w_koszyku.zwieksz_liczbe_sztuk();
                            Console.WriteLine("Produkt " + produkt_w_koszyku.nazwa + " znajdował się już w koszyku. Zwiększono liczbę sztuk w koszyku o 1. Aktualna liczba sztuk wybranego produktu: " + produkt_w_koszyku.liczba_sztuk);
                        }
                        else
                        {
                            koszyk_zalogowanego.dodaj_produkt(new Produkt(wybrany));
                            Console.WriteLine("Dodano do koszyka produkt " + wybrany.nazwa + ".");
                        }
                    }
                    else if(wprowadzony_ciag == "x")
                    {
                        panel_glowny();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Polecenie nierozpoznane.");
                    }
                }
            }

            public void klient_koszyk()
            {
                Console.WriteLine(odstep);
                Console.WriteLine("TWÓJ KOSZYK\n");
                Console.WriteLine("Produkty znajdujące się w twoim koszyku:");
                List<Produkt> wyswietlone_produkty = koszyk_zalogowanego.wyswietl();

                if (wyswietlone_produkty.Count() == 0)
                {
                    Console.WriteLine("Brak produktów do wyświetlenia.");
                }
                if(koszyk_zalogowanego.liczba_rodzajow_utraconych > 0)
                {
                    Console.WriteLine("Ukryto " + koszyk_zalogowanego.liczba_rodzajow_utraconych + " usuniętych z magazynu rodzajów produktów.\n");
                }
                Console.WriteLine();

                while (true)
                {
                    if(wyswietlone_produkty.Count > 0)
                    {
                        Console.Write("Wpisz numer produktu, aby usunąć go z koszyka. Wpisz b, aby dokonać zakupu całej zawartości koszyka. Wpisz c, aby wyczyścić koszyk. ");
                    }
                    Console.WriteLine("Wpisz x, aby wrócić do panelu głównego.");
                    string wprowadzony_ciag = Console.ReadLine();
                    if (uint.TryParse(wprowadzony_ciag, out uint numer_produktu) && --numer_produktu < wyswietlone_produkty.Count())
                    {
                        Produkt wybrany_do_usuniecia = wyswietlone_produkty[(int)numer_produktu];
                        usuwanie_produktu(wybrany_do_usuniecia);
                    }
                    else if (wprowadzony_ciag == "b")
                    {
                        //todo sprawdzenie aktualnej dostepnosci produktow (z uwzglednieniem liczb sztuk)

                        Console.WriteLine("Kwota do zapłaty: " + koszyk_zalogowanego.kwota() + "zł.");
                        Console.Write("Wprowadź kod BLIK: ");
                        Console.ReadLine();
                        Console.WriteLine("Transakcja zakończona pomyślnie. Dziękujemy za zakupy.");
                        koszyk_zalogowanego.czysc();
                        panel_glowny();
                        return;
                    }
                    else if (wprowadzony_ciag == "c")
                    {
                        koszyk_zalogowanego.czysc();
                        Console.WriteLine("Koszyk został wyczyszczony.");
                        panel_glowny();
                        return;
                    }
                    else if (wprowadzony_ciag == "x")
                    {
                        panel_glowny();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Polecenie nierozpoznane.\n");
                    }
                }
            }

            private void usuwanie_produktu(Produkt produkt_do_usuniecia)
            {
                uint liczba_sztuk_do_usuniecia = produkt_do_usuniecia.liczba_sztuk;
                if(produkt_do_usuniecia.liczba_sztuk > 1)
                {
                    Console.WriteLine("Wybrany produkt występuje w liczbie: " + produkt_do_usuniecia.liczba_sztuk + " sztuk. Kliknij enter, aby usunąć wszystkie sztuki tego produktu lub wpisz liczbę sztuk do usunięcia.");
                    string wprowadzony_ciag = Console.ReadLine();
                    if (uint.TryParse(wprowadzony_ciag, out uint podana_liczba) && podana_liczba <= produkt_do_usuniecia.liczba_sztuk)
                    {
                        liczba_sztuk_do_usuniecia = podana_liczba;
                    }
                }
                produkt_do_usuniecia.zmniejsz_liczbe_sztuk(liczba_sztuk_do_usuniecia);
                Console.WriteLine("Usunięto z koszyka " + liczba_sztuk_do_usuniecia + " sztuk produktu " + produkt_do_usuniecia.nazwa + ".");
            }

            public void administrator_zarzadzanie_magazynem()
            {
                magazyn.wyswietl();
                Console.WriteLine("Wpisz numer pozycji, aby edytować produkt. Kliknij enter, aby wrócić do panelu głównego.");
                //todo
            }
        }

        static void Main(string[] args)
        {
            Magazyn magazyn = new Magazyn(Sciezki.magazyn);
            Lista_uzytkownikow uzytkownicy = new Lista_uzytkownikow(Sciezki.uzytkownicy);
            Uzytkownik zalogowany_uzytkownik;

            Interfejs interfejs = new Interfejs(uzytkownicy, magazyn);
            interfejs.panel_glowny();

            uzytkownicy.zapisz_do_pliku(Sciezki.uzytkownicy);
            magazyn.zapisz_do_pliku(Sciezki.magazyn);
            interfejs.koszyk_zalogowanego?.zapisz_do_pliku(Sciezki.koszyk(interfejs.zalogowany_uzytkownik.id));

            Console.WriteLine("\nDane aplikacji zapisane.");
        }
    }
}