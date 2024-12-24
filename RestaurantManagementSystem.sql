-- 1. Kategori Tablosu
CREATE TABLE IF NOT EXISTS kategori (
    kategori_id SERIAL PRIMARY KEY,
    kategori_adi VARCHAR(50) NOT NULL UNIQUE 
);

-- 2. Masa Tablosu
CREATE TABLE IF NOT EXISTS masa (
    masa_id SERIAL PRIMARY KEY,
    kapasite INT NOT NULL CHECK (kapasite IN (2, 4, 5, 6, 8)),
    bolge VARCHAR(50) NOT NULL CHECK (bolge IN ('iç mekan', 'bahçe', 'teras')),
    durum VARCHAR(50) CHECK (durum IN ('bos', 'dolu' , 'rezerve')) DEFAULT 'bos'
);

-- 3. Çalışan Tablosu
CREATE TABLE IF NOT EXISTS calisan (
    calisan_id SERIAL PRIMARY KEY,
    isim VARCHAR(100) NOT NULL,
    soyisim VARCHAR(100) NOT NULL,
    pozisyon VARCHAR(50) NOT NULL CHECK (pozisyon IN ('garson', 'asci', 'personel')),
    iletisim_numarasi VARCHAR(15) UNIQUE NOT NULL CHECK (iletisim_numarasi ~ '^\+90[0-9]{10}$'),
    baslangic_tarihi DATE NOT NULL DEFAULT CURRENT_DATE
);

-- 4. Yemek Tablosu
CREATE TABLE IF NOT EXISTS yemek (
    yemek_id SERIAL PRIMARY KEY,
    isim VARCHAR(100) UNIQUE NOT NULL ,
    fiyat DECIMAL(10, 2) NOT NULL,
    kategori_id INT REFERENCES kategori(kategori_id),
    icerik TEXT
);

CREATE INDEX idx_kategori_adi ON kategori(kategori_adi);
CREATE INDEX idx_yemek_kategori_id ON yemek(kategori_id);

-- 5. Sipariş Tablosu
CREATE TABLE IF NOT EXISTS siparis (
    siparis_id SERIAL PRIMARY KEY,
    masa_id INT NOT NULL REFERENCES masa(masa_id) ON DELETE CASCADE,
    calisan_id INT NOT NULL REFERENCES calisan(calisan_id) ON DELETE SET NULL,
    toplam_tutar DECIMAL(10, 2) DEFAULT 0,
    siparis_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 6. Sipariş Detay Tablosu
CREATE TABLE IF NOT EXISTS siparis_detay (
    siparis_detay_id SERIAL PRIMARY KEY,
    siparis_id INT NOT NULL REFERENCES siparis(siparis_id) ON DELETE CASCADE,
    yemek_id INT NOT NULL REFERENCES yemek(yemek_id) ON DELETE CASCADE,
    miktar INT NOT NULL CHECK (miktar > 0),
    fiyat DECIMAL(10, 2)DEFAULT 0,
    toplam_tutar DECIMAL(10, 2) GENERATED ALWAYS AS (miktar * fiyat) STORED
);

CREATE INDEX idx_siparis_id ON siparis_detay(siparis_id);

-- 7. Ödeme Tablosu
CREATE TABLE IF NOT EXISTS odeme (
    odeme_id SERIAL PRIMARY KEY,
    odeme_tutari DECIMAL(10, 2) DEFAULT 0,
    odeme_turu VARCHAR(50) CHECK (odeme_turu IN ('nakit', 'kredi karti')),
    odeme_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    siparis_id INT NOT NULL REFERENCES siparis(siparis_id) ON DELETE CASCADE
);

-- 8. Rezervasyon Tablosu
CREATE TABLE IF NOT EXISTS rezervasyon (
    rezervasyon_id SERIAL PRIMARY KEY,
    musteri_adSoyad VARCHAR(100) NOT NULL, 
    musteri_telefon VARCHAR(15) NOT NULL CHECK (musteri_telefon ~ '^\+90[0-9]{10}$'),
    masa_id INT REFERENCES masa(masa_id) ON DELETE CASCADE,
    tarih DATE NOT NULL CHECK (tarih BETWEEN CURRENT_DATE AND CURRENT_DATE + INTERVAL '30 days'),
    baslangic_saat TIME NOT NULL,
    bitis_saat TIME NOT NULL CHECK (bitis_saat > baslangic_saat),
    kisi_sayisi INT NOT NULL
);

-- 9. Vardiya Tablosu
CREATE TABLE IF NOT EXISTS vardiya (
    vardiya_id SERIAL PRIMARY KEY,
    calisan_id INT NOT NULL REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    tarih DATE NOT NULL,
    baslangic_saati TIME NOT NULL DEFAULT '13:00:00',
    bitis_saati TIME NOT NULL DEFAULT '22:00:00',
    UNIQUE (calisan_id, tarih),
    CHECK (baslangic_saati < bitis_saati)
);

-- 10. İzin Talebi Tablosu
CREATE TABLE IF NOT EXISTS izin_talebi (
    izin_id SERIAL PRIMARY KEY,
    calisan_id INT NOT NULL REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    baslangic_tarihi DATE NOT NULL,
    bitis_tarihi DATE NOT NULL,
    aciklama TEXT
);

--11.Çalışan ve İzin Talebi Tablosu
CREATE TABLE IF NOT EXISTS calisan_izin (
    calisan_id INT NOT NULL REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    izin_id INT NOT NULL REFERENCES izin_talebi(izin_id) ON DELETE CASCADE,
    PRIMARY KEY (calisan_id, izin_id)
);

-- 12. Garson Tablosu
CREATE TABLE garson (
    calisan_id INT PRIMARY KEY REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    servis_sayisi INT DEFAULT 0
);

-- 13. Aşçı Tablosu 
CREATE TABLE personel (
    calisan_id INT PRIMARY KEY REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    gorev VARCHAR(100) CHECK (gorev IN ('temizlik personeli', 'depo görevlisi', 'bulaşıkçı'))
);

-- 14. Personel Tablosu 
CREATE TABLE asci (
    calisan_id INT PRIMARY KEY REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    deneyim_derecesi VARCHAR(20) CHECK (deneyim_derecesi IN ('yeni', 'orta', 'uzman')) DEFAULT 'yeni'
);

-- 15. Malzeme Tablosu
CREATE TABLE IF NOT EXISTS malzeme (
    malzeme_id SERIAL PRIMARY KEY,
    malzeme_adi VARCHAR(100) NOT NULL UNIQUE,
    miktar INT NOT NULL DEFAULT 0
);

--16. Yemek-Malzeme Tablosu
CREATE TABLE IF NOT EXISTS yemek_malzeme (
    yemek_id INT REFERENCES yemek(yemek_id) ON DELETE CASCADE,
    malzeme_id INT REFERENCES malzeme(malzeme_id) ON DELETE CASCADE,
    PRIMARY KEY (yemek_id, malzeme_id)
);

-- 17. Vardiya-Çalışan Tablosu
CREATE TABLE IF NOT EXISTS vardiya_calisan (
    vardiya_id INT NOT NULL REFERENCES vardiya(vardiya_id) ON DELETE CASCADE,
    calisan_id INT NOT NULL REFERENCES calisan(calisan_id) ON DELETE CASCADE,
    UNIQUE (vardiya_id, calisan_id)
);

-- 18. Silinen Çalışan Tablosu
CREATE TABLE IF NOT EXISTS silinen_calisan (
    silinen_id SERIAL PRIMARY KEY,
    calisan_id INT NOT NULL,
    isim VARCHAR(100) NOT NULL,
    soyisim VARCHAR(100) NOT NULL,
    pozisyon VARCHAR(50) NOT NULL,
    iletisim_numarasi VARCHAR(15),
    baslangic_tarihi DATE, 
    silinme_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- Fonksiyonlar
-- Kategori Ekleme Fonksiyonu
CREATE OR REPLACE FUNCTION ekle_kategori(kategori_adi VARCHAR)
RETURNS VOID AS $$
BEGIN
    INSERT INTO kategori(kategori_adi) VALUES (kategori_adi);
END;
$$ LANGUAGE plpgsql;

-- Kategori Silme Fonksiyonu
CREATE OR REPLACE FUNCTION sil_kategori(p_kategori_id INT)
RETURNS VOID AS $$
BEGIN
    -- Kategoriyi varlığını kontrol et
    IF NOT EXISTS (SELECT 1 FROM kategori WHERE kategori_id = p_kategori_id) THEN
        RAISE EXCEPTION 'Kategori ID % bulunamadı!', p_kategori_id;
    ELSE
        -- Kategori var ise sil
        DELETE FROM kategori WHERE kategori_id = p_kategori_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Kategori Güncelleme Fonksiyonu
CREATE OR REPLACE FUNCTION guncelle_kategori(p_kategori_id INT, p_yeni_kategori_adi VARCHAR)
RETURNS VOID AS $$
BEGIN
    -- Kategorinin varlığını kontrol et
    IF NOT EXISTS (SELECT 1 FROM kategori WHERE kategori_id = p_kategori_id) THEN
        RAISE EXCEPTION 'Kategori ID % bulunamadı!', p_kategori_id;
    ELSE
        -- Kategori var ise güncelle
        UPDATE kategori
        SET kategori_adi = p_yeni_kategori_adi
        WHERE kategori_id = p_kategori_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Masa Ekleme Fonksiyonu
CREATE OR REPLACE FUNCTION ekle_masa(kapasite INTEGER, bolge TEXT, durum TEXT)
RETURNS VOID AS $$
BEGIN
    -- Geçerli bölge kontrolü
    IF bolge NOT IN ('bahçe', 'teras', 'iç mekan') THEN
        RAISE EXCEPTION 'Geçersiz bölge: %! Lütfen "bahçe", "teras" veya "iç mekan" seçin.', bolge;
    -- Geçerli durum kontrolü
    ELSIF durum NOT IN ('boş', 'dolu') THEN
        RAISE EXCEPTION 'Geçersiz durum: %! Lütfen "boş" veya "dolu" seçin.', durum;
    ELSE
        -- Geçerli bölge ve durum ile masa ekleme
        INSERT INTO masa (kapasite, bolge, durum) 
        VALUES (kapasite, bolge, durum);
    END IF;
END;
$$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION ekle_masa(kapasite INTEGER, bolge TEXT, durum TEXT)
RETURNS VOID AS $$
BEGIN
    -- Geçerli bölge kontrolü
    IF bolge NOT IN ('bahçe', 'teras', 'iç mekan') THEN
        RAISE EXCEPTION 'Geçersiz bölge: %! Lütfen "bahçe", "teras" veya "iç mekan" seçin.', bolge;
    -- Geçerli durum kontrolü
    ELSIF durum NOT IN ('bos', 'dolu') THEN
        RAISE EXCEPTION 'Geçersiz durum: %! Lütfen "boş" veya "dolu" seçin.', durum;
    ELSE
        -- Geçerli bölge ve durum ile masa ekleme
        INSERT INTO masa (kapasite, bolge, durum) 
        VALUES (kapasite, bolge, durum);
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Masa Silme Fonksiyonu
CREATE OR REPLACE FUNCTION sil_masa(p_masa_id INTEGER)
RETURNS VOID AS $$
BEGIN
    -- Masa ID'sinin var olup olmadığını kontrol et
    IF NOT EXISTS (SELECT 1 FROM masa WHERE masa_id = p_masa_id) THEN
        RAISE EXCEPTION 'Masa ID % bulunamadı!', p_masa_id;
    ELSE
        -- Masa mevcutsa sil
        DELETE FROM masa WHERE masa_id = p_masa_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Masa Güncelleme Fonksiyonu
CREATE OR REPLACE FUNCTION masa_guncelle(
    p_masa_id INT,
    p_kapasite INT DEFAULT NULL,
    p_bolge VARCHAR DEFAULT NULL,
    p_durum VARCHAR DEFAULT NULL
)
RETURNS VOID AS $$
BEGIN
    -- Geçerli bölge kontrolü
    IF p_bolge IS NOT NULL AND p_bolge NOT IN ('bahçe', 'teras', 'iç mekan') THEN
        RAISE EXCEPTION 'Geçersiz bölge: %! Lütfen "bahçe", "teras" veya "iç mekan" seçin.', p_bolge;
    END IF;

    -- Geçerli durum kontrolü
    IF p_durum IS NOT NULL AND p_durum NOT IN ('bos', 'dolu') THEN
        RAISE EXCEPTION 'Geçersiz durum: %! Lütfen "bos" veya "dolu" seçin.', p_durum;
    END IF;

    UPDATE masa
    SET 
        kapasite = COALESCE(p_kapasite, kapasite),
        bolge = COALESCE(p_bolge, bolge),
        durum = COALESCE(p_durum, durum)
    WHERE masa_id = p_masa_id;

    IF NOT FOUND THEN
        RAISE NOTICE 'Güncelleme yapılacak kayıt bulunamadı: masa_id = %', p_masa_id;
    END IF;
END;
$$ LANGUAGE plpgsql;

-- Malzeme Ekleme Fonksiyonu
CREATE OR REPLACE FUNCTION ekle_malzeme(p_malzeme_adi VARCHAR, p_miktar INT)
RETURNS VOID AS $$
BEGIN
    INSERT INTO malzeme(malzeme_adi, miktar) 
    VALUES (p_malzeme_adi, p_miktar);
END;
$$ LANGUAGE plpgsql;

-- Malzeme Silme Fonksiyonu
CREATE OR REPLACE FUNCTION sil_malzeme(p_malzeme_id INT)
RETURNS VOID AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM malzeme WHERE malzeme_id = p_malzeme_id) THEN
        RAISE EXCEPTION 'Geçersiz malzeme_id: %', p_malzeme_id;
    END IF;

    DELETE FROM malzeme WHERE malzeme_id = p_malzeme_id;
END;
$$ LANGUAGE plpgsql;

-- Malzeme Güncelleme Fonksiyonu
CREATE OR REPLACE FUNCTION guncelle_malzeme(p_malzeme_id INT, p_yeni_miktar INT)
RETURNS VOID AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM malzeme WHERE malzeme_id = p_malzeme_id) THEN
        RAISE EXCEPTION 'Geçersiz malzeme_id: %', p_malzeme_id;
    END IF;

    UPDATE malzeme 
    SET miktar = p_yeni_miktar 
    WHERE malzeme_id = p_malzeme_id;
END;
$$ LANGUAGE plpgsql;

--Trigger için yazılmış fonksiyonlar

-- Kapasite ve bölgelere göre masa sayısını kontrol eden fonksiyon
CREATE OR REPLACE FUNCTION kontrol_masa_detaylari()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.kapasite = 8 AND (SELECT COUNT(*) FROM masa WHERE kapasite = 8) >= 3 THEN
        RAISE EXCEPTION 'Maksimum 8 kişilik masa sayısına ulaşıldı.';
    END IF;

    IF NEW.kapasite = 6 AND (SELECT COUNT(*) FROM masa WHERE kapasite = 6) >= 5 THEN
        RAISE EXCEPTION 'Maksimum 6 kişilik masa sayısına ulaşıldı.';
    END IF;

    IF NEW.kapasite = 5 AND (SELECT COUNT(*) FROM masa WHERE kapasite = 5) >= 4 THEN
        RAISE EXCEPTION 'Maksimum 5 kişilik masa sayısına ulaşıldı.';
    END IF;

    IF NEW.kapasite = 4 AND (SELECT COUNT(*) FROM masa WHERE kapasite = 4) >= 7 THEN
        RAISE EXCEPTION 'Maksimum 4 kişilik masa sayısına ulaşıldı.';
    END IF;

    IF NEW.kapasite = 2 AND (SELECT COUNT(*) FROM masa WHERE kapasite = 2) >= 6 THEN
        RAISE EXCEPTION 'Maksimum 2 kişilik masa sayısına ulaşıldı.';
    END IF;

    IF NEW.bolge = 'bahçe' THEN
        IF (SELECT COUNT(*) FROM masa WHERE bolge = 'bahçe' AND kapasite IN (2, 4)) >= 5 THEN
            RAISE EXCEPTION 'Bahçede toplamda yalnızca 5 masa bulunabilir (2 ve 4 kişilik masalar toplamı).';
        END IF;

        IF NEW.kapasite NOT IN (2, 4) THEN
            RAISE EXCEPTION 'Bahçede yalnızca 2 ve 4 kişilik masalar bulunabilir.';
        END IF;
    END IF;

    IF NEW.bolge = 'teras' THEN
        IF (SELECT COUNT(*) FROM masa WHERE bolge = 'teras') >= 7 THEN
            RAISE EXCEPTION 'Terasda toplamda yalnızca 7 masa bulunabilir.';
        END IF;

        IF NEW.kapasite = 8 AND (SELECT COUNT(*) FROM masa WHERE bolge = 'teras' AND kapasite = 8) >= 1 THEN
            RAISE EXCEPTION 'Terasda maksimum 1 tane 8 kişilik masa bulunabilir.';
        END IF;

        IF NEW.kapasite = 6 AND (SELECT COUNT(*) FROM masa WHERE bolge = 'teras' AND kapasite = 6) >= 1 THEN
            RAISE EXCEPTION 'Terasda maksimum 1 tane 6 kişilik masa bulunabilir.';
        END IF;

        IF NEW.kapasite = 5 AND (SELECT COUNT(*) FROM masa WHERE bolge = 'teras' AND kapasite = 5) >= 2 THEN
            RAISE EXCEPTION 'Terasda maksimum 2 tane 5 kişilik masa bulunabilir.';
        END IF;

        IF NEW.kapasite = 2 AND (SELECT COUNT(*) FROM masa WHERE bolge = 'teras' AND kapasite = 2) >= 5 THEN
            RAISE EXCEPTION 'Terasda maksimum 5 tane 2 kişilik masa bulunabilir.';
        ELSIF NEW.kapasite = 4 AND (SELECT COUNT(*) FROM masa WHERE bolge = 'teras' AND kapasite = 4) >= 4 THEN
            RAISE EXCEPTION 'Terasda maksimum 4 tane 4 kişilik masa bulunabilir.';
        END IF;
    END IF;

    IF NEW.bolge = 'iç mekan' THEN
        IF (SELECT COUNT(*) FROM masa WHERE bolge = 'iç mekan') >= 13 THEN
            RAISE EXCEPTION 'İç mekanda maksimum 13 masa bulunabilir.';
        END IF;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Siparişin toplam tutarını hesaplama fonksiyonu
CREATE OR REPLACE FUNCTION update_toplam_tutar()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE siparis
    SET toplam_tutar = COALESCE((
            SELECT SUM(miktar * fiyat)
            FROM siparis_detay
            WHERE siparis_id = COALESCE(NEW.siparis_id, OLD.siparis_id)  
        ), 0)
    WHERE siparis_id = COALESCE(NEW.siparis_id, OLD.siparis_id); 

    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Ödeme tutarını siparis_detay tablosundan hesaplayan fonksiyon
CREATE OR REPLACE FUNCTION odeme_tutari_ekle()
RETURNS TRIGGER AS $$
BEGIN
    SELECT SUM(toplam_tutar) INTO NEW.odeme_tutari
    FROM siparis_detay
    WHERE siparis_id = NEW.siparis_id;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Fiyatı menu tablosundan çekmek için bir fonksiyon oluşturulur
CREATE OR REPLACE FUNCTION fiyat_bilgisi_ekle()
RETURNS TRIGGER AS $$
BEGIN
    SELECT fiyat INTO NEW.fiyat
    FROM yemek
    WHERE yemek_id = NEW.yemek_id;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Telefon numarası başka bir çalışan tarafından kullanılıyor mu kontrolü
CREATE OR REPLACE FUNCTION calisan_telefon_kontrolu()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM calisan
        WHERE iletisim_numarasi = NEW.iletisim_numarasi
          AND calisan_id <> NEW.calisan_id
    ) THEN
        RAISE EXCEPTION 'Bu telefon numarası (%), başka bir çalışan tarafından kullanılıyor.', NEW.iletisim_numarasi;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Masa kapasitesi ve kişi sayısı kontrolü
CREATE OR REPLACE FUNCTION kontrol_masa_kapasitesi()
RETURNS TRIGGER AS $$
DECLARE
    masa_kapasite INT;
BEGIN
    SELECT kapasite INTO masa_kapasite
    FROM masa
    WHERE masa_id = NEW.masa_id;

    IF masa_kapasite < NEW.kisi_sayisi THEN
        RAISE EXCEPTION 'Seçilen masa kapasitesi (%s), kişi sayısını (%s) karşılayamaz.', masa_kapasite, NEW.kisi_sayisi;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Çalışanın izinli olduğu günlerde vardiya yazılmasını engelleyen ve vardiya sayısının kontrolünü yapan fonksiyon
CREATE OR REPLACE FUNCTION kontrol_vardiya_limiti()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1 
        FROM izin_talebi 
        WHERE calisan_id = NEW.calisan_id 
          AND NEW.tarih BETWEEN baslangic_tarihi AND bitis_tarihi
    ) THEN
        RAISE EXCEPTION 'Çalışan izinli olduğu günlerde vardiya yazılamaz.';
    END IF;

    IF (SELECT COUNT(*) 
        FROM vardiya 
        WHERE calisan_id = NEW.calisan_id 
          AND DATE_PART('year', tarih) = DATE_PART('year', NEW.tarih)
          AND DATE_PART('week', tarih) = DATE_PART('week', NEW.tarih)) >= 5 THEN
        RAISE EXCEPTION 'Bir çalışan haftada en fazla 5 vardiya alabilir.';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Vardiya Tablosundaki Saatlerin Değiştirilmesini Engelleme
CREATE OR REPLACE FUNCTION vardiya_saati_degistirilemez()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.baslangic_saati <> '13:00:00' OR NEW.bitis_saati <> '22:00:00' THEN
        RAISE EXCEPTION 'Başlangıç ve bitiş saati sabittir, değiştirilemez!';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- İzin tarih aralığındaki vardiyaları silme
CREATE OR REPLACE FUNCTION vardiya_sil()
RETURNS TRIGGER AS $$
BEGIN
    DELETE FROM vardiya
    WHERE calisan_id = NEW.calisan_id 
      AND tarih BETWEEN NEW.baslangic_tarihi AND NEW.bitis_tarihi;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Siparişi sadece garsonların alabileceği kontrolü
CREATE OR REPLACE FUNCTION check_garson_calisan()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM calisan WHERE calisan_id = NEW.calisan_id AND pozisyon = 'garson'
    ) THEN
        RAISE EXCEPTION 'Sadece garsonlar sipariş alabilir.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--Rezwervasyon çakışma kontrolünü yapan fonksiyon
CREATE OR REPLACE FUNCTION rezervasyon_cakisma_kontrolu()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM rezervasyon
        WHERE masa_id = NEW.masa_id
          AND tarih = NEW.tarih
          AND (
              (NEW.baslangic_saat, NEW.bitis_saat) OVERLAPS (baslangic_saat, bitis_saat)
          )
          AND rezervasyon_id != NEW.rezervasyon_id -- Aynı rezervasyonu kontrol dışı bırak
    ) THEN
        RAISE EXCEPTION 'Bu masa için belirtilen saatler arasında başka bir rezervasyon mevcut!';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--Eğer yeni bir garson eklenmişse, garson tablosuna da veri ekleyen fonksiyon
CREATE OR REPLACE FUNCTION insert_garson_data() 
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.pozisyon = 'garson' THEN
        INSERT INTO garson (calisan_id, servis_sayisi) 
        VALUES (NEW.calisan_id, 0); 
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--Eğer yeni bir aşçı eklenmişse, aşçı tablosuna da veri ekleyen fonksiyon
CREATE OR REPLACE FUNCTION insert_asci_data() 
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.pozisyon = 'asci' THEN
        INSERT INTO asci (calisan_id, deneyim_derecesi) 
        VALUES (NEW.calisan_id, 'yeni');
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--Eğer yeni bir personel eklenmişse, personel tablosuna rastgele görev ekleyen fonksiyon
CREATE OR REPLACE FUNCTION insert_personel_data() 
RETURNS TRIGGER AS $$
DECLARE
    random_gorev VARCHAR(100);
BEGIN
    IF NEW.pozisyon = 'personel' THEN
        random_gorev := CASE
            WHEN random() < 0.33 THEN 'temizlik personeli'
            WHEN random() < 0.66 THEN 'depo görevlisi'
            ELSE 'bulaşıkçı'
        END;

        INSERT INTO personel (calisan_id, gorev) 
        VALUES (NEW.calisan_id, random_gorev); 
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Garson tablosundaki servis sayısını sipariş alma durumuna göre güncelleyen fonksiyon
CREATE OR REPLACE FUNCTION garson_servis_sayisi_guncelle()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE garson
    SET servis_sayisi = (
        SELECT COUNT(*) FROM siparis WHERE calisan_id = NEW.calisan_id
    )
    WHERE calisan_id = NEW.calisan_id;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--Yeni bir yemek eklenmişse ve içeriğinde yeni bir malzeme varsa malzeme tablosuna malzemeleri ekleyen fonksiyon
CREATE OR REPLACE FUNCTION yemek_malzeme_ekle()
RETURNS TRIGGER AS $$
DECLARE
    v_malzeme_adi TEXT;  
    malzeme_exists INT;
    v_malzeme_id INT;
BEGIN
    FOR v_malzeme_adi IN
        SELECT unnest(string_to_array(NEW.icerik, ',')) 
    LOOP
        SELECT malzeme_id INTO v_malzeme_id
        FROM malzeme
        WHERE malzeme.malzeme_adi = v_malzeme_adi
        LIMIT 1;
        
        IF NOT FOUND THEN
            INSERT INTO malzeme (malzeme_adi, miktar)
            VALUES (v_malzeme_adi, 0) RETURNING malzeme_id INTO v_malzeme_id;
        END IF;

        INSERT INTO yemek_malzeme (yemek_id, malzeme_id)
        VALUES (NEW.yemek_id, v_malzeme_id); 
    END LOOP;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Çalışan Silindiğinde silinen_calisan tablosuna ekleyen fonksiyon
CREATE OR REPLACE FUNCTION calisan_sil_trigger()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO silinen_calisan (calisan_id, isim, soyisim, pozisyon, iletisim_numarasi, baslangic_tarihi)
    VALUES (OLD.calisan_id, OLD.isim, OLD.soyisim, OLD.pozisyon, OLD.iletisim_numarasi, OLD.baslangic_tarihi);
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-- Rezervasyon ekleme işlemi sonrasında masa durumunu güncelleyen fonksiyon
CREATE OR REPLACE FUNCTION masa_durum_guncelle()
RETURNS TRIGGER AS $$
BEGIN
    IF (
        (NEW.tarih = CURRENT_DATE AND NEW.baslangic_saat > CURRENT_TIME)
        OR
        (NEW.tarih > CURRENT_DATE)
    ) THEN
        UPDATE masa
        SET durum = 'rezerve'
        WHERE masa_id = NEW.masa_id;
    END IF;

    IF (
        NEW.tarih = CURRENT_DATE 
        AND NEW.baslangic_saat <= CURRENT_TIME
        AND NEW.bitis_saat > CURRENT_TIME
    ) THEN
        UPDATE masa
        SET durum = 'dolu'
        WHERE masa_id = NEW.masa_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Rezervasyon silme işlemi sonrasında masa durumunu güncelleyen fonksiyon
CREATE OR REPLACE FUNCTION masa_durum_bosalt()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM rezervasyon
        WHERE masa_id = OLD.masa_id
        AND (
            (tarih = CURRENT_DATE AND baslangic_saat > CURRENT_TIME)
            OR
            (tarih = CURRENT_DATE AND bitis_saat > CURRENT_TIME)
            OR
            (tarih > CURRENT_DATE)
        )
    ) THEN
        UPDATE masa
        SET durum = 'bos'
        WHERE masa_id = OLD.masa_id;
    ELSE
        UPDATE masa
        SET durum = 'rezerve'
        WHERE masa_id = OLD.masa_id;
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-- Rezercasyondaki tarih, saat aralığı ve isim içeriği kontrolü
CREATE OR REPLACE FUNCTION validate_rezervasyon()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.musteri_adSoyad !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s]+$' THEN
        RAISE EXCEPTION 'Musteri AdSoyad sadece harf ve boşluk içerebilir: %', NEW.musteri_adSoyad;
    END IF;

    IF NEW.baslangic_saat < TIME '13:00:00' OR NEW.baslangic_saat > TIME '22:00:00' THEN
        RAISE EXCEPTION 'Başlangıç saati 13:00:00 ile 22:00:00 arasında olmalı: %', NEW.baslangic_saat;
    END IF;

    IF NEW.bitis_saat < TIME '13:00:00' OR NEW.bitis_saat > TIME '22:00:00' THEN
        RAISE EXCEPTION 'Bitiş saati 13:00:00 ile 22:00:00 arasında olmalı: %', NEW.bitis_saat;
    END IF;

    IF NEW.baslangic_saat >= NEW.bitis_saat THEN
        RAISE EXCEPTION 'Başlangıç saati, bitiş saatinden önce olmalı: % >= %', NEW.baslangic_saat, NEW.bitis_saat;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- siparis_detay tablosuna ekleme yaparken siparis_id kontrolü
CREATE OR REPLACE FUNCTION validate_siparis_id()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM siparis WHERE siparis_id = NEW.siparis_id) THEN
        RAISE EXCEPTION 'Geçersiz siparis_id: %. siparis tablosunda bulunamadı.', NEW.siparis_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- odeme tablosuna ekleme yaparken siparis_id kontrolü
CREATE OR REPLACE FUNCTION validate_odeme_siparis_id()
RETURNS TRIGGER AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM siparis WHERE siparis_id = NEW.siparis_id) THEN
        RAISE EXCEPTION 'Geçersiz siparis_id: %. siparis tablosunda bulunamadı.', NEW.siparis_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Yemek tablosuna ekleme yaparken ismi ve içeriği kontrol eden fonksiyon
CREATE OR REPLACE FUNCTION validate_yemek_fields()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.isim !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s]+$' THEN
        RAISE EXCEPTION 'Yemek ismi sadece harf ve boşluk içerebilir: %', NEW.isim;
    END IF;

    IF NEW.icerik IS NOT NULL AND NEW.icerik !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s,]+$' THEN
        RAISE EXCEPTION 'Yemek içeriği sadece harf, boşluk ve virgül içerebilir: %', NEW.icerik;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Kategori tablosuna ekleme yaparken kategori adı kontrolü
CREATE OR REPLACE FUNCTION validate_kategori_adi()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.kategori_adi !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s]+$' THEN
        RAISE EXCEPTION 'Kategori adı sadece harf ve boşluk içerebilir: %', NEW.kategori_adi;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Malzeme tablosuna ekleme yaparken malzeme adı kontrolü
CREATE OR REPLACE FUNCTION validate_malzeme_adi()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.malzeme_adi !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s]+$' THEN
        RAISE EXCEPTION 'Malzeme adı sadece harf ve boşluk içerebilir: %', NEW.malzeme_adi;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Kategori silme işlemi yaparken kategoriye ait yemek var mı kontrolü
CREATE OR REPLACE FUNCTION validate_kategori_delete()
RETURNS TRIGGER AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM yemek WHERE kategori_id = OLD.kategori_id) THEN
        RAISE EXCEPTION 'Bu kategoriye ait yemekler var, silme işlemi yapılamaz: %', OLD.kategori_adi;
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-- Ödeme yaparken sipariş numarası ve daha önce ödeme yapılmış mı kontrolü
CREATE OR REPLACE FUNCTION validate_siparis_id_for_odeme()
RETURNS TRIGGER AS $$
BEGIN
    -- Eğer siparis_id siparis tablosunda bulunmuyorsa hata ver
    IF NOT EXISTS (SELECT 1 FROM siparis WHERE siparis_id = NEW.siparis_id) THEN
        RAISE EXCEPTION 'Sipariş ID % siparis tablosunda bulunmamaktadır.', NEW.siparis_id;
    END IF;

    -- Eğer ödeme eklenmek istenen siparis_id'ye sahip zaten bir ödeme varsa hata ver
    IF EXISTS (SELECT 1 FROM odeme WHERE siparis_id = NEW.siparis_id) THEN
        RAISE EXCEPTION 'Bu sipariş için zaten ödeme yapılmış: %', NEW.siparis_id;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Çalışan isim ve soyisim kontrolü
CREATE OR REPLACE FUNCTION validate_calisan_isim_soyisim()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.isim !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s]+$' THEN
        RAISE EXCEPTION 'Çalışan ismi sadece harf ve boşluk içerebilir: %', NEW.isim;
    END IF;

    IF NEW.soyisim !~ '^[A-Za-zÇĞİÖŞÜçğıöşü\s]+$' THEN
        RAISE EXCEPTION 'Çalışan soyismi sadece harf ve boşluk içerebilir: %', NEW.soyisim;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Rezervasyon tarihi kontrolü
CREATE OR REPLACE FUNCTION validate_rezervasyon_tarihi()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.tarih < CURRENT_DATE THEN
        RAISE EXCEPTION 'Rezervasyon tarihi bugünden önce olamaz: %', NEW.tarih;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- İzin talebi tarihleri kontrolü
CREATE OR REPLACE FUNCTION validate_izin_talebi_tarihleri()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.baslangic_tarihi > NEW.bitis_tarihi THEN
        RAISE EXCEPTION 'Başlangıç tarihi, bitiş tarihinden sonra olamaz: % - %', NEW.baslangic_tarihi, NEW.bitis_tarihi;
    END IF;

    IF NEW.baslangic_tarihi <= CURRENT_DATE OR NEW.bitis_tarihi <= CURRENT_DATE THEN
        RAISE EXCEPTION 'Başlangıç ve bitiş tarihi bugünden sonraki bir tarih olmalı: % - %', NEW.baslangic_tarihi, NEW.bitis_tarihi;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

--TRIGGERLAR

CREATE TRIGGER masa_kontrolu
BEFORE INSERT ON masa
FOR EACH ROW
EXECUTE FUNCTION kontrol_masa_detaylari();

CREATE TRIGGER check_rezervasyon
BEFORE INSERT OR UPDATE ON rezervasyon
FOR EACH ROW
EXECUTE FUNCTION validate_rezervasyon();

CREATE TRIGGER check_rezervasyon_tarihi
BEFORE INSERT OR UPDATE ON rezervasyon
FOR EACH ROW
EXECUTE FUNCTION validate_rezervasyon_tarihi();

CREATE TRIGGER kontrol_masa_kapasitesi_trigger
BEFORE INSERT ON rezervasyon
FOR EACH ROW
EXECUTE FUNCTION kontrol_masa_kapasitesi();

CREATE TRIGGER tr_masa_durum_guncelle
AFTER INSERT ON rezervasyon
FOR EACH ROW
EXECUTE FUNCTION masa_durum_guncelle();

CREATE TRIGGER tr_masa_durum_bosalt
AFTER DELETE ON rezervasyon
FOR EACH ROW
EXECUTE FUNCTION masa_durum_bosalt();

CREATE TRIGGER check_kategori_adi
BEFORE INSERT OR UPDATE ON kategori
FOR EACH ROW
EXECUTE FUNCTION validate_kategori_adi();

CREATE TRIGGER check_kategori_delete
BEFORE DELETE ON kategori
FOR EACH ROW
EXECUTE FUNCTION validate_kategori_delete();

CREATE TRIGGER check_malzeme_adi
BEFORE INSERT OR UPDATE ON malzeme
FOR EACH ROW
EXECUTE FUNCTION validate_malzeme_adi();

CREATE TRIGGER check_yemek_fields
BEFORE INSERT OR UPDATE ON yemek
FOR EACH ROW
EXECUTE FUNCTION validate_yemek_fields();

CREATE TRIGGER check_siparis_id
BEFORE INSERT OR UPDATE ON siparis_detay
FOR EACH ROW
EXECUTE FUNCTION validate_siparis_id();

CREATE TRIGGER check_garson_trigger
BEFORE INSERT OR UPDATE ON siparis
FOR EACH ROW
EXECUTE FUNCTION check_garson_calisan();

CREATE TRIGGER update_siparis_toplam_tutar
AFTER INSERT OR UPDATE OR DELETE ON siparis_detay
FOR EACH ROW
EXECUTE FUNCTION update_toplam_tutar();

CREATE TRIGGER check_siparis_id_for_odeme
BEFORE INSERT OR UPDATE ON odeme
FOR EACH ROW
EXECUTE FUNCTION validate_siparis_id_for_odeme();

CREATE TRIGGER check_odeme_siparis_id
BEFORE INSERT OR UPDATE ON odeme
FOR EACH ROW
EXECUTE FUNCTION validate_odeme_siparis_id();

CREATE TRIGGER odeme_tutari_trigger
BEFORE INSERT OR UPDATE ON odeme
FOR EACH ROW
EXECUTE FUNCTION odeme_tutari_ekle();

CREATE TRIGGER fiyat_bilgisi_trigger
BEFORE INSERT OR UPDATE ON siparis_detay
FOR EACH ROW
EXECUTE FUNCTION fiyat_bilgisi_ekle();

CREATE TRIGGER vardiya_saati_trigger
BEFORE INSERT OR UPDATE ON vardiya
FOR EACH ROW
EXECUTE FUNCTION vardiya_saati_degistirilemez();

CREATE TRIGGER vardiya_limiti
BEFORE INSERT ON vardiya
FOR EACH ROW
EXECUTE FUNCTION kontrol_vardiya_limiti();

CREATE TRIGGER check_izin_talebi_tarihleri
BEFORE INSERT OR UPDATE ON izin_talebi
FOR EACH ROW
EXECUTE FUNCTION validate_izin_talebi_tarihleri();

CREATE TRIGGER izin_verildiginde_vardiya_sil
AFTER INSERT OR UPDATE ON izin_talebi
FOR EACH ROW
EXECUTE FUNCTION vardiya_sil();

CREATE TRIGGER check_calisan_isim_soyisim
BEFORE INSERT OR UPDATE ON calisan
FOR EACH ROW
EXECUTE FUNCTION validate_calisan_isim_soyisim();

CREATE TRIGGER telefon_numarasi_kontrolu
BEFORE INSERT OR UPDATE ON calisan
FOR EACH ROW
EXECUTE FUNCTION calisan_telefon_kontrolu();

CREATE TRIGGER after_insert_calisan
AFTER INSERT ON calisan
FOR EACH ROW
EXECUTE FUNCTION insert_garson_data();

CREATE TRIGGER after_insert_calisan_asci
AFTER INSERT ON calisan
FOR EACH ROW
EXECUTE FUNCTION insert_asci_data();

CREATE TRIGGER after_insert_calisan_personel
AFTER INSERT ON calisan
FOR EACH ROW
EXECUTE FUNCTION insert_personel_data();

CREATE TRIGGER siparis_servis_sayisi_guncelle
AFTER INSERT OR DELETE ON siparis
FOR EACH ROW
EXECUTE FUNCTION garson_servis_sayisi_guncelle();

CREATE TRIGGER rezervasyon_cakisma_trigger
BEFORE INSERT OR UPDATE ON rezervasyon
FOR EACH ROW
EXECUTE FUNCTION rezervasyon_cakisma_kontrolu();

CREATE TRIGGER yemek_malzeme_ekle_trigger
AFTER INSERT ON yemek
FOR EACH ROW
EXECUTE FUNCTION yemek_malzeme_ekle();

CREATE TRIGGER calisan_sil_trigger
AFTER DELETE OR UPDATE ON calisan
FOR EACH ROW
EXECUTE FUNCTION calisan_sil_trigger();

INSERT INTO kategori (kategori_adi) 
VALUES 
('Sıcak Başlangıç'),
('Soğuk Başlangıç'),
('Çorbalar'),
('Ana Yemekler'),
('Salatalar');

INSERT INTO yemek (isim, fiyat, kategori_id, icerik)
VALUES
('Paçanga Böreği', 200.00, 1, 'yufka, domates, yeşil biber, kırmızı kapya biber, kaşar peyniri, pastırma'),
('Haşlama İçli Köfte', 180.00, 1, 'Kaburga eti, but eti, ince bulgur, süzme yoğurt'),
('Fındık Lahmacun', 90.00, 1, 'döş eti, biber, maydonoz, sarımsak'),
('Vişneli Hurmalı Sıcak Humus', 450.00, 1, 'Nohut, Konya Bozkır tahini, Ayvalık zeytinyağı, Limon, Vişne, Hurma'),
('Hatay Usulü Sıcak Humus', 400.00, 1, 'Nohut, Konya Bozkır tahini, Ayvalık zeytinyağı, Limon'),
('Zeytinyağlı Kuru Patlıcan Dolması', 170.00, 2, 'Antep patlıcanı, pirinç, maydonoz, sumak'),
('Vişneli Yaprak Sarma', 230.00, 2, 'Asma yaprağı, vişne, pirinç, kuru soğan, zeytinyağı, nane, dereotu, tuz, karabiber'),
('Soğan Dolması', 170.00, 2, 'soğan, kıyma, pirinç, domates salçası, zeytinyağı, tuz, karabiber, nane, maydanoz'),
('Şefin Zeytinyağlı Seçimi', 460.00, 2, 'tuz'),
('Mercimek Çorbası', 100.00, 3, 'kırmızı mercimek, kuru soğan, havuç, patates, un, zeytinyağı, tereyağı, tuz, karabiber, kırmızı toz biber'),
('Ezogelin Çorbası', 100.00, 3, 'kırmızı mercimek, bulgur, pirinç, kuru soğan, domates salçası, biber salçası, tereyağı, zeytinyağı, nane, pul biber, tuz, karabiber'),
('Terbiyeli Tavuk Çorbası', 150.00, 3, 'tavuk budu, şehriye, zeytinyağı, yoğurt, un, yumurta, sarımsak, Limon, karabiber, tuz'),
('Yeşil Bahçe Salatası', 300.00, 5, 'Akdeniz yeşillikleri, roka kuzu kulağı, sumak, kuru nane, limon, nar ekşisi, zeytinyağı, sos'),
('Kaşık Salata', 320.00, 5, 'Domates, salatalık, soğan, maydonoz, biber, ceviz nar ekişisi, zeytinyağı'),
('Peynirli Roka Salatası', 330.00, 5, 'Roka, Erzincan tulumu, domates, narekşisi, zeytinyağı, limon'),
('Vişneli Kinoa Salatası', 360.00, 5, 'Vişne, Avokado, Akdeniz Yeşillikleri, Kinoa Zeytinyağı, Balzemik, Nar Ekşisi'),
('Tavuklu Roka Salatası', 410.00, 5, 'Roka, erzincan tulumu, soya soslu göğüs tavuk, özel salata sosu'),
('Etli Akdeniz Salatası', 500.00, 5, 'Roka, erzincan tulumu, soya soslu dana bonfile, özel salata sosu'),
('Edirne Tava Ciğeri', 570.00, 4, 'Ciğer'),
('Etli Kuru Fasulye', 380.00, 4, 'Erzurum İspir kuru fasulyesi, kuşbaşı et'),
('Kuzu Tandır', 820.00, 4, 'Kuzu'),
('Tavuk Çökertme', 480.00, 4, 'Tavuk bonfile, kibrit Patates, yoğurt, domates sos'),
('Çökertme Kebabı', 600.00, 4, 'Dana bonfile, kibrit Patates, yoğurt, domates sos'),
('Ala Nazik Kıymadan', 600.00, 4, 'Közlenmiş patlıcan, tereyağı, süzme yoğurt, zırh kıyma kebabı'),
('Zırh Kıyma Acılı', 500.00, 4, 'Dana kuzu but eti, kuzu kaburga, kuzu kuyruk yağı, albiber, tuz'),
('Yoğurtlu Kebap', 550.00, 4, 'Zırh Kıyma Kebabı, Kemik Sulu Tırnak Pide, Süzme Yoğurt, Tereyağı, Sos'),
('Kayseri Mantısı', 380.00, 4, 'Tuz'),
('Kuzu Şiş', 640.00, 4, 'Kuzu but'),
('Mantar Soslu Antrikot', 700.00, 4, 'Mantar, Dana Antrikot'),
('Izgara Köfte', 500.00, 4, 'Dana Kıyma'),
('Tavuk Şiş', 450.00, 4, 'Tavuk Göğsü'),
('Şalgam', 80.00, 6, 'Şalgam'),
('Yayık Ayran', 75.00, 6, 'Yoğurt'),
('Küçük su', 35.00, 6, 'Küçük su'),
('Büyük su', 80.00, 6, 'Büyük su'),
('Sade Soda', 60.00, 6, 'Sade Soda'),
('Meyveli Soda', 60.00, 6, 'Meyveli Soda'),
('Türk Kahvesi', 90.00, 6, 'Türk Kahvesi'),
('Çay', 45.00, 6, 'Çay'),
('Gazlı İçecekler', 90.00, 6, 'Gazlı İçecekler');


INSERT INTO masa (kapasite, bolge, durum)
VALUES
(6, 'iç mekan', 'bos'),
(4, 'iç mekan', 'bos'),
(4, 'bahçe', 'bos'),
(6, 'iç mekan', 'bos'),
(2, 'iç mekan', 'bos'),
(6, 'teras', 'bos'),
(8, 'teras', 'bos'),
(2, 'iç mekan', 'bos'),
(4, 'iç mekan', 'bos'),
(4, 'bahçe', 'bos'),
(6, 'iç mekan', 'bos'),
(8, 'iç mekan', 'bos'),
(6, 'iç mekan', 'bos'),
(2, 'bahçe', 'bos'),
(4, 'teras', 'bos');

INSERT INTO calisan (isim, soyisim, pozisyon, iletisim_numarasi, baslangic_tarihi)
VALUES
('Emircan', 'Ağaç', 'asci', '+905345787345', '2024-12-21'),
('Zeynep', 'Bıçakcı', 'garson', '+904632472537', '2024-12-21'),
('Berat', 'Dere', 'personel', '+902438732687', '2024-12-21'),
('Azra', 'Balkaya', 'garson', '+904238746238', '2024-12-21'),
('Ceren', 'Albayrak', 'personel', '+903295743875', '2024-12-21'),
('Yusuf', 'Alemdaroğlu', 'asci', '+904938247938', '2024-12-21'),
('Esra', 'Nergis', 'garson', '+904324987583', '2024-12-21'),
('Bahar', 'Karagözoğlu', 'garson', '+905843579348', '2024-12-21'),
('Sude', 'Vergili', 'personel', '+909382759843', '2024-12-21'),
('Melisa', 'Çıkrıkcı', 'garson', '+904395873948', '2024-12-21'),
('Egemen', 'Yıldız', 'asci', '+903948573487', '2024-12-21'),
('Selen', 'Güler', 'garson', '+903928759437', '2024-12-21');

INSERT INTO siparis (masa_id, calisan_id)
VALUES
(1, 2),
(2, 2),
(3, 4),
(4, 4),
(5, 8),
(6, 7),
(7, 7),
(8, 8),
(9, 12),
(10, 10);

INSERT INTO malzeme (malzeme_adi, miktar)
VALUES
('Kıyma', 120),
('Tavuk Göğsü', 100),
('Pirinç', 150);

INSERT INTO vardiya (calisan_id, tarih)
VALUES
(1, '2024-12-21'),
(2, '2024-12-21'),
(3, '2024-12-21'),
(4, '2024-12-21'),
(5, '2024-12-21'),
(6, '2024-12-21'),
(7, '2024-12-21'),
(8, '2024-12-21'),
(9, '2024-12-21'),
(10, '2024-12-21');

INSERT INTO rezervasyon (musteri_adSoyad, musteri_telefon, masa_id, tarih, baslangic_saat, bitis_saat, kisi_sayisi)
VALUES
('Ahmet Yılmaz', '+905321234567', 1, '2024-12-28', '13:00:00', '15:00:00', 2),
('Elif Demir', '+905323456789', 2, '2024-12-23', '14:00:00', '16:00:00', 4),
('Fatma Kılıç', '+905324567890', 3, '2024-12-24', '15:00:00', '17:00:00', 3),
('Murat Çelik', '+905325678901', 4, '2024-12-25', '16:00:00', '18:00:00', 5),
('Zeynep Arslan', '+905326789012', 5, '2024-12-29', '17:00:00', '19:00:00', 2),
('Mehmet Gül', '+905327890123', 6, '2024-12-26', '18:00:00', '20:00:00', 6),
('Emine Kaya', '+905328901234', 7, '2024-12-27', '19:00:00', '21:00:00', 8),
('Ali Şahin', '+905329012345', 8, '2024-12-27', '20:00:00', '22:00:00', 2),
('Özge Aydın', '+905320123456', 11, '2024-12-24', '13:00:00', '15:00:00', 4),
('Gökhan Koç', '+905321234567', 12, '2024-12-25', '14:00:00', '16:00:00', 2),
('Ayşe Yıldız', '+905322345678', 13, '2024-12-23', '15:00:00', '17:00:00', 6),
('Canan Güngör', '+905323456789', 14, '2024-12-30', '16:00:00', '18:00:00', 2),
('Kemal Ekinci', '+905324567890', 15, '2024-12-25', '17:00:00', '19:00:00', 4),
('Nihan Bayraktar', '+905325678901', 9, '2024-12-28', '18:00:00', '20:00:00', 3),
('Baran Yılmaz', '+905326789012', 10, '2024-12-27', '19:00:00', '21:00:00', 4);