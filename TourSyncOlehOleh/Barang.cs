using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AplikasiKasir
{
    public class Barang
    {
        public int Id { get; set; }
        public string NamaBarang { get; set; }
        public decimal Harga { get; set; }
        public int Stok { get; set; }

        public void MenuStok(AppDbContext db)
        {
            bool kembali = false;
            while (!kembali)
            {
                Console.Clear();
                Console.WriteLine("--- MANAJEMEN STOK BARANG ---");
                Console.WriteLine("1. Lihat Daftar Barang & Stok");
                Console.WriteLine("2. Tambah Barang Baru");
                Console.WriteLine("3. Edit / Update Stok & Harga");
                Console.WriteLine("4. Hapus Barang");
                Console.WriteLine("5. Kembali ke Menu Utama");
                Console.Write("Pilih [1-5]: ");
                string p = Console.ReadLine();

                if (p == "1")
                {
                    TampilkanTabelBarang(db);
                    Jeda();
                }
                else if (p == "2")
                {
                    Console.Clear();
                    Console.WriteLine("=== TAMBAH BARANG BARU ===");

                    int id;
                    while (true)
                    {
                        Console.Write("ID Barang    : ");
                        string inputId = Console.ReadLine();
                        if (int.TryParse(inputId, out id) && id > 0)
                        {
                            if (db.Barangs.Any(b => b.Id == id))
                            {
                                Console.WriteLine($"ID {id} sudah digunakan! Silakan gunakan ID lain.");
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("ID harus berupa angka bulat positif (misal: 1, 2, 101)!");
                        }
                    }

                    Console.Write("Nama Barang  : ");
                    string nama = Console.ReadLine();
                    while (string.IsNullOrWhiteSpace(nama))
                    {
                        Console.Write("Nama barang tidak boleh kosong! Masukkan nama: ");
                        nama = Console.ReadLine();
                    }

                    decimal harga;
                    Console.Write("Harga Satuan : ");
                    while (!decimal.TryParse(Console.ReadLine(), out harga) || harga < 0)
                    {
                        Console.Write("Harga tidak valid! Masukkan angka yang benar (Rp): ");
                    }

                    int stok;
                    Console.Write("Jumlah Stok  : ");
                    while (!int.TryParse(Console.ReadLine(), out stok) || stok < 0)
                    {
                        Console.Write("Jumlah stok tidak valid! Masukkan angka bulat: ");
                    }

                    db.Barangs.Add(new Barang
                    {
                        Id = id,
                        NamaBarang = nama,
                        Harga = harga,
                        Stok = stok
                    });
                    db.SaveChanges();

                    Console.WriteLine($"\nBarang '{nama}' (ID: {id}) berhasil ditambahkan!");
                    Jeda();
                }
                else if (p == "3")
                {
                    Console.Clear();
                    Console.WriteLine("=== EDIT BARANG / STOK ===");
                    TampilkanTabelBarang(db);

                    Console.Write("\nMasukkan ID Barang yang akan diedit: ");
                    if (int.TryParse(Console.ReadLine(), out int id))
                    {
                        var b = db.Barangs.Find(id);
                        if (b != null)
                        {
                            Console.Write($"Nama baru ({b.NamaBarang}) : "); string nama = Console.ReadLine();
                            Console.Write($"Harga baru ({b.Harga:N0}) : "); string hargaStr = Console.ReadLine();
                            Console.Write($"Stok baru ({b.Stok})   : "); string stokStr = Console.ReadLine();

                            if (!string.IsNullOrWhiteSpace(nama)) b.NamaBarang = nama;
                            if (!string.IsNullOrWhiteSpace(hargaStr) && decimal.TryParse(hargaStr, out decimal h)) b.Harga = h;
                            if (!string.IsNullOrWhiteSpace(stokStr) && int.TryParse(stokStr, out int s)) b.Stok = s;

                            db.SaveChanges();
                            Console.WriteLine("\nData barang berhasil diperbarui!");
                        }
                        else
                        {
                            Console.WriteLine("\nBarang dengan ID tersebut tidak ditemukan.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nID tidak valid.");
                    }
                    Jeda();
                }
                else if (p == "4")
                {
                    Console.Clear();
                    Console.WriteLine("=== HAPUS BARANG ===");
                    TampilkanTabelBarang(db);

                    Console.Write("\nMasukkan ID Barang yang akan dihapus: ");
                    if (int.TryParse(Console.ReadLine(), out int id))
                    {
                        var b = db.Barangs.Find(id);
                        if (b != null)
                        {
                            Console.Write($"Yakin ingin menghapus '{b.NamaBarang}'? (y/t): ");
                            if (Console.ReadLine().Trim().ToLower() == "y")
                            {
                                db.Barangs.Remove(b);
                                db.SaveChanges();
                                Console.WriteLine("\nBarang berhasil dihapus!");
                            }
                            else
                            {
                                Console.WriteLine("\nPenghapusan dibatalkan.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nBarang dengan ID tersebut tidak ditemukan.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nID tidak valid.");
                    }
                    Jeda();
                }
                else if (p == "5")
                {
                    kembali = true;
                }
            }
        }

        public void TampilkanTabelBarang(AppDbContext db)
        {
            Console.Clear();
            Console.WriteLine("==================================================================");
            Console.WriteLine(string.Format("| {0,-5} | {1,-25} | {2,15} | {3,6} |", "ID", "Nama Barang", "Harga Satuan", "Stok"));
            Console.WriteLine("==================================================================");

            var list = db.Barangs.ToList();
            if (!list.Any())
            {
                Console.WriteLine(string.Format("| {0,-62} |", "Belum ada data barang. Silakan tambah barang baru."));
            }
            else
            {
                foreach (var b in list)
                {
                    string hargaFormatted = $"Rp {b.Harga:N0}";
                    Console.WriteLine(string.Format("| {0,-5} | {1,-25} | {2,15} | {3,6} |", b.Id, b.NamaBarang, hargaFormatted, b.Stok));
                }
            }
            Console.WriteLine("==================================================================");
        }

        public void Jeda()
        {
            Console.WriteLine("\nTekan Enter untuk melanjutkan...");
            Console.ReadLine();
        }
    }
}
