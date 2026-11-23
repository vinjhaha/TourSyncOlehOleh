using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AplikasiKasir
{
    public class Transaksi
    {
        public int Id { get; set; }
        public string NamaBarang { get; set; }
        public int JumlahBeli { get; set; }
        public decimal TotalHarga { get; set; }
        public decimal UangBayar { get; set; }
        public decimal Kembalian { get; set; }
        public DateTime Tanggal { get; set; } = DateTime.Now;

        // ALUR TRANSAKSI PEMBAYARAN KASIR LANGSUNG
        public void MulaiTransaksiLangsung(AppDbContext db)
        {
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("           KASIR & TRANSAKSI PEMBELIAN            ");
            Console.WriteLine("==================================================");

            var semuaBarang = db.Barangs.ToList();
            if (!semuaBarang.Any())
            {
                Console.WriteLine("\n[PERINGATAN] Katalog barang masih kosong!");
                Console.WriteLine("Silakan tambahkan data barang terlebih dahulu di Menu 1 (Manajemen Stok Barang).\n");
                new Barang().Jeda();
                return;
            }

            var keranjang = new List<(Barang BarangObj, int Qty)>();
            Barang barangHelper = new Barang();

            bool lanjutTambah = true;
            while (lanjutTambah)
            {
                Console.Clear();
                Console.WriteLine("==================================================");
                Console.WriteLine("           KASIR & TRANSAKSI PEMBELIAN            ");
                Console.WriteLine("==================================================");

                // Tampilkan daftar barang yang tersedia
                Console.WriteLine("\n--- DAFTAR BARANG YANG TERSEDIA ---");
                Console.WriteLine("==================================================================");
                Console.WriteLine(string.Format("| {0,-5} | {1,-25} | {2,15} | {3,6} |", "ID", "Nama Barang", "Harga Satuan", "Stok"));
                Console.WriteLine("==================================================================");
                foreach (var b in db.Barangs.ToList())
                {
                    string hFormatted = $"Rp {b.Harga:N0}";
                    Console.WriteLine(string.Format("| {0,-5} | {1,-25} | {2,15} | {3,6} |", b.Id, b.NamaBarang, hFormatted, b.Stok));
                }
                Console.WriteLine("==================================================================");

                // Tampilkan keranjang jika sudah ada item
                if (keranjang.Any())
                {
                    Console.WriteLine("\n[ KERANJANG BELANJA SAAT INI ]");
                    decimal subTotal = 0;
                    foreach (var k in keranjang)
                    {
                        decimal sub = k.BarangObj.Harga * k.Qty;
                        subTotal += sub;
                        Console.WriteLine($"- {k.BarangObj.NamaBarang} ({k.Qty}x @Rp {k.BarangObj.Harga:N0}) = Rp {sub:N0}");
                    }
                    Console.WriteLine($"Total Sementara: Rp {subTotal:N0}");
                    Console.WriteLine("--------------------------------------------------");
                }

                Console.Write("\nMasukkan ID Barang yang ingin dibeli: ");
                if (!int.TryParse(Console.ReadLine(), out int idBarang))
                {
                    Console.WriteLine("ID tidak valid! Tekan Enter...");
                    Console.ReadLine();
                    continue;
                }

                var barangPilihan = db.Barangs.Find(idBarang);
                if (barangPilihan == null)
                {
                    Console.WriteLine("Barang dengan ID tersebut tidak ditemukan! Tekan Enter...");
                    Console.ReadLine();
                    continue;
                }

                if (barangPilihan.Stok <= 0)
                {
                    Console.WriteLine($"Stok barang '{barangPilihan.NamaBarang}' habis! Tekan Enter...");
                    Console.ReadLine();
                    continue;
                }

                Console.Write($"Jumlah Beli (Stok tersedia: {barangPilihan.Stok}): ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                {
                    Console.WriteLine("Jumlah beli tidak valid! Tekan Enter...");
                    Console.ReadLine();
                    continue;
                }

                int qtyDiKeranjang = keranjang.Where(k => k.BarangObj.Id == idBarang).Sum(k => k.Qty);
                if ((qty + qtyDiKeranjang) > barangPilihan.Stok)
                {
                    Console.WriteLine($"Stok tidak mencukupi! Sisa stok yang bisa dibeli: {barangPilihan.Stok - qtyDiKeranjang}. Tekan Enter...");
                    Console.ReadLine();
                    continue;
                }

                // Masuk keranjang
                keranjang.Add((barangPilihan, qty));
                Console.WriteLine($"\n-> {barangPilihan.NamaBarang} (x{qty}) berhasil masuk keranjang!");

                Console.Write("\nApakah ingin menambah barang lain? (y/t): ");
                string jawab = Console.ReadLine().Trim().ToLower();
                if (jawab != "y")
                {
                    lanjutTambah = false;
                }
            }

            if (!keranjang.Any())
            {
                Console.WriteLine("\nTransaksi dibatalkan.");
                barangHelper.Jeda();
                return;
            }

            // PROSES PEMBAYARAN LANGSUNG
            Console.Clear();
            Console.WriteLine("==================================================");
            Console.WriteLine("                 PEMBAYARAN KASIR                 ");
            Console.WriteLine("==================================================");
            decimal grandTotal = 0;
            foreach (var k in keranjang)
            {
                decimal sub = k.BarangObj.Harga * k.Qty;
                grandTotal += sub;
                Console.WriteLine($"- {k.BarangObj.NamaBarang}\n  {k.Qty}x @Rp {k.BarangObj.Harga:N0} = Rp {sub:N0}");
            }
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"GRAND TOTAL : Rp {grandTotal:N0}");
            Console.WriteLine("--------------------------------------------------");

            decimal uangBayar = 0;
            while (true)
            {
                Console.Write("Masukkan Uang Pembayaran (Rp): ");
                string inputBayar = Console.ReadLine();
                if (decimal.TryParse(inputBayar, out uangBayar))
                {
                    if (uangBayar >= grandTotal)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Uang pembayaran kurang! Dibutuhkan minimal Rp {grandTotal:N0}.");
                        Console.Write("Batalkan transaksi? (y/t): ");
                        if (Console.ReadLine().Trim().ToLower() == "y")
                        {
                            Console.WriteLine("\nTransaksi dibatalkan.");
                            barangHelper.Jeda();
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Nominal uang tidak valid!");
                }
            }

            decimal kembalian = uangBayar - grandTotal;
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine($"TUNAI       : Rp {uangBayar:N0}");
            Console.WriteLine($"KEMBALIAN   : Rp {kembalian:N0}");
            Console.WriteLine("--------------------------------------------------");

            DateTime waktuTransaksi = DateTime.Now;
            foreach (var k in keranjang)
            {
                decimal sub = k.BarangObj.Harga * k.Qty;
                k.BarangObj.Stok -= k.Qty;

                db.Transaksis.Add(new Transaksi
                {
                    NamaBarang = k.BarangObj.NamaBarang,
                    JumlahBeli = k.Qty,
                    TotalHarga = sub,
                    UangBayar = uangBayar,
                    Kembalian = kembalian,
                    Tanggal = waktuTransaksi
                });
            }
            db.SaveChanges();

            try
            {
                string pathPdf = CetakStrukPdf(keranjang, grandTotal, uangBayar, kembalian, waktuTransaksi);
                Console.WriteLine($"\n[SUKSES] Transaksi Berhasil Disimpan!");
                Console.WriteLine($"Struk PDF tersimpan di: {pathPdf}");
                Console.WriteLine("Membuka struk PDF...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nTransaksi tersimpan, tetapi gagal cetak PDF: {ex.Message}");
            }

            barangHelper.Jeda();
        }

        // STRUK PDF SESUAI GAMBAR CONTOH MINI E-COMMERCE
        public string CetakStrukPdf(List<(Barang BarangObj, int Qty)> keranjang, decimal grandTotal, decimal uangBayar, decimal kembalian, DateTime waktu)
        {
            string namaFile = $"Struk_{waktu:yyyyMMdd_HHmmss}.pdf";
            string pathLengkap = Path.GetFullPath(namaFile);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A7);
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Text("=== MINI E-COMMERCE ===").AlignCenter().Bold().FontSize(11);

                    page.Content().Column(col =>
                    {
                        col.Spacing(3);
                        col.Item().Text($"Tanggal: {waktu:dd/MM/yyyy HH:mm}");
                        col.Item().Text(new string('-', 32));

                        foreach (var k in keranjang)
                        {
                            decimal sub = k.BarangObj.Harga * k.Qty;
                            col.Item().Text($"{k.BarangObj.NamaBarang}\n  {k.Qty}x @Rp {k.BarangObj.Harga:N0} = Rp {sub:N0}");
                        }

                        col.Item().Text(new string('-', 32));
                        col.Item().Text($"GRAND TOTAL : Rp {grandTotal:N0}").Bold();
                        col.Item().Text($"TUNAI       : Rp {uangBayar:N0}");
                        col.Item().Text($"KEMBALIAN   : Rp {kembalian:N0}");
                        col.Item().Text(new string('-', 32));
                        col.Item().Text("Terima Kasih Telah Berbelanja!").AlignCenter().Italic();
                    });
                });
            })
            .GeneratePdf(pathLengkap);

            // Buka struk PDF secara otomatis di Foxit Reader / PDF Viewer default
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo(pathLengkap)
                {
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch { }

            return pathLengkap;
        }

        public void LihatRekap(AppDbContext db)
        {
            Console.Clear();
            Console.WriteLine("=== REKAP & RIWAYAT PENJUALAN ===");
            var rekapList = db.Transaksis.ToList();

            if (!rekapList.Any())
            {
                Console.WriteLine("Belum ada transaksi penjualan.");
            }
            else
            {
                decimal totalOmzet = 0;
                foreach (var t in rekapList)
                {
                    Console.WriteLine($"[{t.Tanggal:dd/MM/yyyy HH:mm}] {t.NamaBarang} (x{t.JumlahBeli}) - Total: Rp {t.TotalHarga:N0} | Bayar: Rp {t.UangBayar:N0} | Kembali: Rp {t.Kembalian:N0}");
                    totalOmzet += t.TotalHarga;
                }
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"TOTAL OMZET PENJUALAN : Rp {totalOmzet:N0}");
            }

            Barang barangHelper = new Barang();
            barangHelper.Jeda();
        }
    }
}
