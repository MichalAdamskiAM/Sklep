using System;

namespace Sklep
{
    class Program
    {
        class Klient
        {
            private string email;
            private string haslo;

            public Klient(string email, string haslo)
            {
                this.email = email;
                this.haslo = haslo;
            }


        }

        class Lista_klientow
        {
            private List<Klient> klienci;

            public Lista_klientow()
            {
                klienci = new List<Klient>();
            }
        }

        class Produkt
        {

            private string nazwa;
            private uint liczba_sztuk;
            private double cena;

            Produkt(string nazwa,uint liczba_sztuk)
            {
                this.nazwa = nazwa;
                this.liczba_sztuk = liczba_sztuk;
            }
            public string Nazwa {
                get { return nazwa; }
            }
            public double Liczba_sztuk
            {
                get { return liczba_sztuk; }
            }

            public bool Set_liczba_Sztuk(int o_ile_zmnienic)
            {
                if(liczba_sztuk-o_ile_zmnienic>0)
                {
                    liczba_sztuk = (uint)((int)liczba_sztuk+o_ile_zmnienic);
                    return true;
                }
                else
                {
                    liczba_sztuk = 0;
                    return false;
                }
            }
        }

        static void Main(string[] args)
        {

            //
            //costam@gmail.com; maslo; chleb
            //inny@gmail.com; suszarka; dzem; 


        }
    }
}