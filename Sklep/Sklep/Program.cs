using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Sklep
{
    class Program
    {
        static class Tekst
        {
            static public string rozsun(string cz1, string cz2)
            {
                int liczba = 100 - cz1.Length - cz2.Length;
                string odstep = "";
                for (int i = 0; i < liczba; i++)
                {
                    odstep += ' ';
                }
                return cz1 + odstep + cz2;
            }
        }

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
        }

        class Administrator : Uzytkownik
        {
            public Administrator(string wiersz) : base(wiersz) { }
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

                StreamReader odczyt = new StreamReader(sciezka, System.Text.Encoding.UTF8);
                string? wiersz;

                while((wiersz = odczyt.ReadLine()) is not null)
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
                odczyt.Close();
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
                using (StreamWriter zapis = new StreamWriter(sciezka))
                {
                    foreach (Uzytkownik uzytkownik in uzytkownicy)
                    {
                        zapis.WriteLine(uzytkownik.format_do_pliku());
                    }
                }
            }
        }

        class Produkt
        {
            public uint id { get; }
            public string nazwa { get; set; }
            public uint liczba_sztuk { get; private set; }
            public double cena { get; set; }

            public Produkt(uint id, string nazwa, uint liczba_sztuk, double cena)
            {
                this.id = id;
                this.nazwa = nazwa;
                this.liczba_sztuk = liczba_sztuk;
                this.cena = cena;
            }

            public Produkt(Produkt kopiowany, uint liczba_sztuk = 1)
            {
                this.id = kopiowany.id;
                this.nazwa = kopiowany.nazwa;
                this.liczba_sztuk = liczba_sztuk;
                this.cena = kopiowany.cena;
            }

            public void zwieksz_liczbe_sztuk(uint o_ile = 1)
            {
                liczba_sztuk += o_ile;
            }

            public void zmniejsz_liczbe_sztuk(uint o_ile = 1)
            {
                if (liczba_sztuk - o_ile >= 0)
                {
                    liczba_sztuk -= o_ile;
                }
                else
                {
                    liczba_sztuk = 0;
                }
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

        class Zbior_produktow
        {
            public List<Produkt> produkty { get; protected set; }

            public Zbior_produktow()
            {
                produkty = new List<Produkt>();
            }

            public Zbior_produktow(List<Produkt> produkty)
            {
                this.produkty = produkty;
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

            public List<Produkt> wyswietl(string wyszukiwanie = "", bool pokaz_0_sztuk = false)
            {
                int i = 1;
                List<Produkt> wynik = new List<Produkt>();
                foreach (Produkt produkt in produkty)
                {
                    if (produkt.nazwa.Contains(wyszukiwanie) && (produkt.liczba_sztuk > 0 || pokaz_0_sztuk))
                    {
                        Console.WriteLine(" " + i++ + ". " + produkt.nazwa + " - " + produkt.cena + " zł. Liczba sztuk: " + produkt.liczba_sztuk + ".");
                        wynik.Add(produkt);
                    }
                }
                return wynik;
            }

            public string podsumowanie_zakupu()
            {
                string wynik = "";
                string cz1, cz2;
                foreach(Produkt produkt in produkty)
                {
                    if(produkt.liczba_sztuk != 0)
                    {
                        cz1 = " " + produkt.nazwa + " " + produkt.liczba_sztuk + "*" + produkt.cena;
                        cz2 = produkt.liczba_sztuk * produkt.cena + "\n";
                        wynik += Tekst.rozsun(cz1, cz2);
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

            public void wykupiono(List<Produkt> wykupione_produkty)
            {
                foreach (Produkt wykupiony_produkt in wykupione_produkty)
                {
                    this.znajdz_po_id(wykupiony_produkt.id)?.zmniejsz_liczbe_sztuk(wykupiony_produkt.liczba_sztuk);
                }
            }

            public double kwota()
            {
                double kwota = 0;
                foreach (Produkt produkt in produkty)
                {
                    kwota += produkt.cena * produkt.liczba_sztuk;
                }
                return kwota;
            }
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

                StreamReader odczyt = new StreamReader(sciezka, System.Text.Encoding.UTF8);
                string? wiersz;
                string nazwa;
                string[] wiersz_tab;
                uint id = 0, liczba_sztuk;
                double cena;

                while ((wiersz = odczyt.ReadLine()) is not null)
                {
                    wiersz_tab = wiersz.Split(';');
                    id = uint.Parse(wiersz_tab[0]);
                    nazwa = wiersz_tab[1];
                    liczba_sztuk = uint.Parse(wiersz_tab[2]);
                    cena = double.Parse(wiersz_tab[3]);

                    produkty.Add(new Produkt(id, nazwa, liczba_sztuk, cena));
                }
                odczyt.Close();
                nastepne_wolne_id = id + 1;
            }

            public void dodaj_produkt(string nazwa, uint liczba_sztuk, double cena)
            {
                produkty.Add(new Produkt(nastepne_wolne_id++, nazwa, liczba_sztuk, cena));
            }

            public List<Produkt> produkty_niedostepne(List<Produkt> sprawdzane_produkty)
            {
                List<Produkt> wynik = new List<Produkt>();
                foreach (Produkt sprawdzany_produkt in sprawdzane_produkty)
                {
                    Produkt? odpowiednik_w_magazynie = this.znajdz_po_id(sprawdzany_produkt.id);
                    if (odpowiednik_w_magazynie?.liczba_sztuk < sprawdzany_produkt.liczba_sztuk)
                    {
                        wynik.Add(new Produkt(sprawdzany_produkt, sprawdzany_produkt.liczba_sztuk - odpowiednik_w_magazynie.liczba_sztuk));
                    }
                }
                return wynik;
            }

            public List<Produkt> produkty_dostepne(List<Produkt> sprawdzane_produkty)
            {
                List<Produkt> wynik = new List<Produkt>();
                foreach (Produkt sprawdzany_produkt in sprawdzane_produkty)
                {
                    Produkt? odpowiednik_w_magazynie = this.znajdz_po_id(sprawdzany_produkt.id);
                    if (odpowiednik_w_magazynie?.liczba_sztuk < sprawdzany_produkt.liczba_sztuk)
                    {
                        wynik.Add(new Produkt(sprawdzany_produkt, odpowiednik_w_magazynie.liczba_sztuk));
                    }
                    else
                    {
                        wynik.Add(new Produkt(sprawdzany_produkt, sprawdzany_produkt.liczba_sztuk));
                    }
                }
                return wynik;
            }

            public void zapisz_do_pliku(string sciezka)
            {
                using (StreamWriter zapis = new StreamWriter(sciezka))
                {
                    foreach (Produkt produkt in produkty)
                    {
                        zapis.WriteLine(produkt.format_do_pliku_magazyn());
                    }
                }
            }
        }

        class Koszyk : Zbior_produktow
        {
            public uint liczba_utraconych_rodzajow { get; protected set; } = 0;

            public Koszyk(string sciezka, Magazyn magazyn)
            {
                produkty = new List<Produkt>();

                if (!File.Exists(sciezka))
                {
                    return;
                }

                StreamReader odczyt = new StreamReader(sciezka, System.Text.Encoding.UTF8);
                string? wiersz;
                string nazwa;
                string[] wiersz_tab;
                uint id, liczba_sztuk;
                double cena;
                Produkt? rodzaj_wczytywanego_produktu;

                while ((wiersz = odczyt.ReadLine()) is not null)
                {
                    wiersz_tab = wiersz.Split(';');
                    id = uint.Parse(wiersz_tab[0]);
                    liczba_sztuk = uint.Parse(wiersz_tab[1]);

                    rodzaj_wczytywanego_produktu = magazyn.znajdz_po_id(id);
                    if (rodzaj_wczytywanego_produktu is null)
                    {
                        liczba_utraconych_rodzajow++;
                        continue;
                    }

                    nazwa = rodzaj_wczytywanego_produktu.nazwa;
                    cena = rodzaj_wczytywanego_produktu.cena;

                    produkty.Add(new Produkt(id, nazwa, liczba_sztuk, cena));
                }
                odczyt.Close();
            }

            public void zapisz_do_pliku(string sciezka)
            {
                using (StreamWriter zapis = new StreamWriter(sciezka))
                {
                    foreach (Produkt produkt in produkty)
                    {
                        zapis.WriteLine(produkt.format_do_pliku_koszyk());
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
                    Console.Write("Wybierz akcję:\n l - Logowanie.\n r - Utworzenie nowego konta.\nWpisz polecenie: ");
                    akcja = Console.ReadLine()!;

                    if(akcja == "l")
                    {
                        logowanie(uzytkownicy);
                    }
                    else if(akcja == "r")
                    {
                        rejestracja(uzytkownicy);
                    }
                    else
                    {
                        Console.WriteLine("Polecenie nierozpoznane.\n");
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
                    email = Console.ReadLine()!;
                    Console.Write("Hasło: ");
                    haslo = Console.ReadLine()!;

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
                    email = Console.ReadLine()!;
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
                haslo = Console.ReadLine()!;

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
                    Console.Write("Wybierz akcję:\n m - Wyświetl produkty dostępne w sklepie.\n x - Zapisz dane i zamknij aplikację.\nWpisz polecenie: ");
                    akcja = Console.ReadLine()!;
                    if(akcja == "m")
                    {
                        administrator_zarzadzanie_magazynem();
                        return;
                    }
                    else if(akcja == "x")
                    {
                        return;
                    }
                    Console.WriteLine("Polecenie nierozpoznane.");
                }
            }

            public void panel_klienta()
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - PANEL GŁÓWNY KLIENTA\n");
                string akcja;
                while (true)
                {
                    Console.Write("Wybierz akcję:\n m - Wyświetl wszystkie produkty dostępne w sklepie.\n s - Szukaj produktu.\n k - Wyświetl swój koszyk/Dokonaj zakupu.\n x - Zapisz dane i zamknij aplikację.\nWpisz polecenie: ");
                    akcja = Console.ReadLine()!;
                    if (akcja == "m")
                    {
                        klient_dostepne_produkty();
                        return;
                    }
                    else if (akcja == "s")
                    {
                        Console.Write("Szukaj: ");
                        string wyszukiwanie = Console.ReadLine()!;
                        klient_dostepne_produkty(wyszukiwanie);
                        return;
                    }
                    else if (akcja == "k")
                    {
                        klient_koszyk();
                        return;
                    }
                    else if (akcja == "x")
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
                    Console.WriteLine("Produkty związane z ''" + wyszukiwanie + "'' dostępne aktualnie w sklepie:");
                }
                else
                {
                    Console.WriteLine("Produkty dostępne aktualnie w sklepie:");
                }
                List<Produkt> wyswietlone_produkty = magazyn.wyswietl(wyszukiwanie);
                if (wyswietlone_produkty.Count == 0)
                {
                    Console.WriteLine("Brak produktów do wyświetlenia.");
                }

                while (true)
                {
                    Console.Write("\nWybierz akcję:\n <numer produktu> - Dodaj produkt do koszyka (po spacji dopisz liczbę sztuk, aby dodać więcej niż jedną sztukę).\n x - Powrót do panelu głównego.\nWpisz polecenie: ");
                    string wprowadzony_ciag = Console.ReadLine()!;
                    int lokalizacja_separatora = wprowadzony_ciag.IndexOf(' ');
                    if(lokalizacja_separatora == -1)
                    {
                        lokalizacja_separatora = wprowadzony_ciag.Length;
                    }
                    string mozliwy_numer = wprowadzony_ciag[..lokalizacja_separatora];
                    if (uint.TryParse(mozliwy_numer, out uint numer_produktu) && --numer_produktu < wyswietlone_produkty.Count)
                    {
                        Produkt wybrany = wyswietlone_produkty[(int)numer_produktu];
                        Produkt? produkt_w_koszyku = koszyk_zalogowanego.znajdz_po_id(wybrany.id);

                        if (!uint.TryParse(wprowadzony_ciag[lokalizacja_separatora..], out uint liczba_sztuk_do_dodania))
                        {
                            liczba_sztuk_do_dodania = 1;
                        }

                        //Brak zabezpieczenia przed dodaniem do koszyka liczby sztuk przekraczajacej liczbe sztuk w magazynie jest zamierzony, gdyz uzytkownik moze planowac zakupic wiecej sztuk i czekac na ich dodanie do magazynu, aby wtedy dokonać zakupu. Proba zakupu zbyt duzej liczby sztuk zostanie udaremniona przez funkcje klient_koszyk()

                        if (produkt_w_koszyku is not null)
                        {
                            produkt_w_koszyku.zwieksz_liczbe_sztuk(liczba_sztuk_do_dodania);
                        }
                        else
                        {
                            koszyk_zalogowanego.dodaj_produkt(new Produkt(wybrany));
                            produkt_w_koszyku = koszyk_zalogowanego.znajdz_po_id(wybrany.id);
                        }

                        if(produkt_w_koszyku?.liczba_sztuk > liczba_sztuk_do_dodania)
                        {
                            Console.WriteLine("Produkt " + produkt_w_koszyku.nazwa + " znajdował się już w koszyku. Zwiększono liczbę sztuk w koszyku o " + liczba_sztuk_do_dodania + ". Aktualna liczba sztuk wybranego produktu w koszyku: " + produkt_w_koszyku.liczba_sztuk + ".");
                        }
                        else
                        {
                            Console.WriteLine("Dodano do koszyka " + liczba_sztuk_do_dodania + " sztuk produktu " + wybrany.nazwa + ".");
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

                if (wyswietlone_produkty.Count == 0)
                {
                    Console.WriteLine("Brak produktów do wyświetlenia.");
                }
                if(koszyk_zalogowanego.liczba_utraconych_rodzajow > 0)
                {
                    Console.WriteLine("Ukryto " + koszyk_zalogowanego.liczba_utraconych_rodzajow + " usuniętych z magazynu rodzajów produktów.");
                }
                Console.WriteLine();

                while (true)
                {
                    if(wyswietlone_produkty.Count > 0)
                    {
                        Console.Write("Wybierz akcję:\n <numer produktu> - Usuń produkt z koszyka.\n z - Zakup całej zawartości koszyka.\n c - Usunięcie całej zawartości koszyka\n x - Powrót do panelu głównego.\nWpisz polecenie: ");
                    }
                    else
                    {
                        Console.Write("Wybierz akcję:\n x - Powrót do panelu głównego.\nWpisz polecenie: ");
                    }
                    string wprowadzony_ciag = Console.ReadLine()!;
                    if (uint.TryParse(wprowadzony_ciag, out uint numer_produktu) && --numer_produktu < wyswietlone_produkty.Count)
                    {
                        Produkt wybrany_do_usuniecia = wyswietlone_produkty[(int)numer_produktu];
                        usuwanie_produktu(wybrany_do_usuniecia);
                        return;
                    }
                    else if (wprowadzony_ciag == "z")
                    {
                        Zbior_produktow produkty_niedostepne = new Zbior_produktow(magazyn.produkty_niedostepne(koszyk_zalogowanego.produkty));
                        if(produkty_niedostepne.produkty.Count > 0)
                        {
                            Console.WriteLine("\nPoniższe produkty z twojego koszyka są aktualnie niedostępne.");
                            produkty_niedostepne.wyswietl();

                            while (true)
                            {
                                Console.Write("\nWybierz akcję:\n z - Zakup dostępnych produktów.\n x - Powrót do panelu głównego.\nWpisz polecenie: ");
                                string akcja = Console.ReadLine()!;
                                if (akcja == "z")
                                {
                                    break;
                                }
                                else if (akcja == "x")
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

                        Zbior_produktow kupowane_produkty = new Zbior_produktow(magazyn.produkty_dostepne(koszyk_zalogowanego.produkty));
                        Console.WriteLine("\nPodsumowanie zakupu:");
                        string podsumowanie_zakupu = kupowane_produkty.podsumowanie_zakupu();
                        if(podsumowanie_zakupu != "")
                        {
                            Console.Write(podsumowanie_zakupu);
                            Console.Write(" --------------------------------------------------------------------------------------------------");
                            Console.WriteLine(Tekst.rozsun("\n Kwota do zapłaty (PLN): ", kupowane_produkty.kwota().ToString()));
                            Console.Write("\nWprowadź kod BLIK: ");
                            Console.ReadLine();
                            Console.WriteLine("Transakcja zakończona pomyślnie. Dziękujemy za zakupy.");
                            magazyn.wykupiono(kupowane_produkty.produkty);
                            koszyk_zalogowanego.wykupiono(kupowane_produkty.produkty);
                        }
                        else
                        {
                            Console.WriteLine("Brak produktów do zakupienia. Wybierz produkty z dostępnych w sklepie.");
                        }
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
                    Console.Write("Wybrany produkt występuje w liczbie: " + produkt_do_usuniecia.liczba_sztuk + " sztuk.\nWybierz akcję:\n Kliknij enter, aby usunąć wszystkie sztuki tego produktu\n Wpisz liczbę sztuk do usunięcia.\nWpisz polecenie:");
                    string wprowadzony_ciag = Console.ReadLine()!;
                    if (uint.TryParse(wprowadzony_ciag, out uint podana_liczba) && podana_liczba <= produkt_do_usuniecia.liczba_sztuk)
                    {
                        liczba_sztuk_do_usuniecia = podana_liczba;
                    }
                }
                produkt_do_usuniecia.zmniejsz_liczbe_sztuk(liczba_sztuk_do_usuniecia);
                Console.WriteLine("Usunięto z koszyka " + liczba_sztuk_do_usuniecia + " sztuk produktu " + produkt_do_usuniecia.nazwa + ". W koszyku pozostało " + produkt_do_usuniecia.liczba_sztuk + " sztuk tego produktu.");
                klient_koszyk();
                return;
            }

            public void administrator_zarzadzanie_magazynem(string wyszukiwanie = "")
            {
                Console.WriteLine(odstep);
                Console.WriteLine("SKLEP - MAGAZYN\n");
                if (wyszukiwanie != "")
                {
                    Console.WriteLine("Produkty związane z ''" + wyszukiwanie + "'' dostępne aktualnie w sklepie:");
                }
                else
                {
                    Console.WriteLine("Produkty dostępne aktualnie w sklepie:");
                }
                List<Produkt> wyswietlone_produkty = magazyn.wyswietl(wyszukiwanie, true);
                Console.WriteLine();

                while (true)
                {
                    Console.Write("\nWybierz akcję:\n <numer produktu> - Edytuj produkt.\n + - Dodaj nowy produkt.\n x - Powrót do panelu głównego.\nWpisz polecenie: ");
                    string wprowadzony_ciag = Console.ReadLine()!;
                    if (uint.TryParse(wprowadzony_ciag, out uint numer_produktu) && --numer_produktu < wyswietlone_produkty.Count)
                    {
                        Produkt wybrany = wyswietlone_produkty[(int)numer_produktu];
                        edytuj_produkt(wybrany);
                        return;
                    }
                    else if (wprowadzony_ciag == "+")
                    {
                        dodawanie_produktu();
                        return;
                    }
                    else if (wprowadzony_ciag == "x")
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

            private void edytuj_produkt(Produkt edytowany_produkt)
            {
                Console.WriteLine(odstep);
                Console.WriteLine("EDYCJA PRODUKTU " + edytowany_produkt.nazwa);

                while (true)
                {
                    Console.Write("\nWybierz akcję:\n n - Zmień nazwę produktu.\n + - Zwiększ liczbę sztuk w magazynie.\n c - Zmień cenę produktu.\n x - Powrót do magazynu.\nWpisz polecenie: ");
                    string wprowadzony_ciag = Console.ReadLine()!;

                    if (wprowadzony_ciag == "n")
                    {
                        Console.Write("Podaj nową nazwę produktu: ");
                        edytowany_produkt.nazwa = Console.ReadLine()!;
                        Console.WriteLine("Nazwa produktu zmieniona.");
                        administrator_zarzadzanie_magazynem();
                        return;
                    }
                    if (wprowadzony_ciag == "+")
                    {
                        string wprowadzony_ciag_liczba;
                        while (true)
                        {
                            Console.Write("Podaj liczbę sztuk dodanych do magazynu: ");
                            wprowadzony_ciag_liczba = Console.ReadLine()!;
                            if (uint.TryParse(wprowadzony_ciag_liczba, out uint poprawna_liczba))
                            {
                                edytowany_produkt.zwieksz_liczbe_sztuk(poprawna_liczba);
                                Console.WriteLine("Liczba sztuk zmieniona.");
                                administrator_zarzadzanie_magazynem();
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Niepoprawna liczba sztuk.");
                            }
                        }
                    }
                    if (wprowadzony_ciag == "c")
                    {
                        string wprowadzony_ciag_cena;
                        while (true)
                        {
                            Console.Write("Podaj nową cenę: ");
                            wprowadzony_ciag_cena = Console.ReadLine()!;
                            if (double.TryParse(wprowadzony_ciag_cena, out double poprawna_cena))
                            {
                                edytowany_produkt.cena = poprawna_cena;
                                Console.WriteLine("Cena zmieniona.");
                                administrator_zarzadzanie_magazynem();
                                return;
                            }
                            else
                            {
                                Console.WriteLine("Niepoprawna cena.");
                            }
                        }
                    }
                    else if (wprowadzony_ciag == "x")
                    {
                        administrator_zarzadzanie_magazynem();
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Polecenie nierozpoznane.");
                    }
                }
            }

            private void dodawanie_produktu()
            {
                string nazwa, wprowadzony_ciag;
                uint liczba_sztuk;
                double cena;

                Console.WriteLine(odstep);
                Console.WriteLine("DODAWANIE NOWEGO PRODUKTU\n");

                Console.Write("Nazwa: ");
                nazwa = Console.ReadLine()!;

                while (true)
                {
                    Console.Write("Liczba sztuk: ");
                    wprowadzony_ciag = Console.ReadLine()!;
                    if (uint.TryParse(wprowadzony_ciag, out liczba_sztuk))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Niepoprawna liczba sztuk.");
                    }
                }

                while (true)
                {
                    Console.Write("Cena: ");
                    wprowadzony_ciag = Console.ReadLine()!;
                    if (double.TryParse(wprowadzony_ciag, out cena)) //UWAGA separatorem dziesietnym jest przecinek, nie kropka
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Niepoprawna cena.");
                    }
                }

                magazyn.dodaj_produkt(nazwa, liczba_sztuk, cena);
                administrator_zarzadzanie_magazynem();
            }
        }

        static void Main(string[] args)
        {
            Magazyn magazyn = new Magazyn(Sciezki.magazyn);
            Lista_uzytkownikow uzytkownicy = new Lista_uzytkownikow(Sciezki.uzytkownicy);

            Interfejs interfejs = new Interfejs(uzytkownicy, magazyn);
            interfejs.panel_glowny();

            uzytkownicy.zapisz_do_pliku(Sciezki.uzytkownicy);
            magazyn.zapisz_do_pliku(Sciezki.magazyn);
            interfejs.koszyk_zalogowanego?.zapisz_do_pliku(Sciezki.koszyk(interfejs.zalogowany_uzytkownik.id));

            Console.WriteLine("\nDane aplikacji zapisane.");
        }
    }
}