using System;
using System.Xml.Linq;

namespace Sklep
{
    class Program
    {
        class Uzytkownik
        {
            public uint id { get; }
            protected string email;
            protected string haslo;

            public Uzytkownik(string wiersz) {
                string[] wiersz_tab = wiersz.Split(';');
                id = uint.Parse(wiersz_tab[0]);
                email = wiersz_tab[1];
                haslo = wiersz_tab[2];
            }
        }

        class Klient : Uzytkownik
        {
            public Klient(string wiersz) : base(wiersz) { }
        }

        class Lista_uzytkownikow
        {
            private List<Uzytkownik> uzytkownicy;
            public uint liczba_uzytkownikow { get; private set; }

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
                    uzytkownicy.Add(new Uzytkownik(wiersz));
                }
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

        static void Main(string[] args)
        {
            const string sciezka_uzytkownicy = "../../../../dane/uzytkownicy.csv";
            const string sciezka_koszyki = "../../../../dane/koszyk"; //+ id + ".csv"
            string email, haslo;

            Lista_uzytkownikow uzytkownicy = new Lista_uzytkownikow(sciezka_uzytkownicy);

            Console.WriteLine("LOGOWANIE");
            Console.Write("Email: ");
            email = Console.ReadLine();
            Console.Write("Hasło: ");
            haslo = Console.ReadLine();







            Console.ReadKey();
        }
    }
}