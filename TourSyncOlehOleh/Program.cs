using System;
using QuestPDF.Infrastructure;

namespace AplikasiKasir
{
    class Program
    {
        static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            // Membuat instance objek dari class Barang dan Transaksi
            Barang barangObj = new Barang();
            Transaksi transaksiObj = new Transaksi();

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                bool selesai = false;
                while (!selesai)
                {
                    Console.Clear();
                    Console.WriteLine("=====================================");
                    Console.WriteLine("    APLIKASI MINI  KASIR ");
                    Console.WriteLine("=====================================");
                    Console.WriteLine("1. Manajemen Stok UMKM");
                    Console.WriteLine("2. Kasir & Transaksi Pembelian");
                    Console.WriteLine("3. Lihat Rekap Penjualan (Omzet)");
                    Console.WriteLine("4. Keluar Program");
                    Console.Write("Pilih menu [1-4]: ");
                    string pilihan = Console.ReadLine();

                    if (pilihan == "1")
                    {
                        barangObj.MenuStok(db);
                    }
                    else if (pilihan == "2")
                    {
                        transaksiObj.MulaiTransaksiLangsung(db);
                    }
                    else if (pilihan == "3")
                    {
                        transaksiObj.LihatRekap(db);
                    }
                    else if (pilihan == "4")
                    {
                        selesai = true;
                        Console.WriteLine("\nTerima kasih telah menggunakan aplikasi kasir.");
                    }
                    else
                    {
                        Console.WriteLine("\nPilihan tidak valid! Tekan Enter...");
                        Console.ReadLine();
                    }
                }
            }
        }
    }
}
