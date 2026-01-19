-- À exécuter dans GestionParfum pour corriger les parfums sans catégorie
USE GestionParfum;

-- On prend la première catégorie existante comme fallback
DECLARE @FallbackCat INT = (SELECT TOP 1 Id FROM Categories ORDER BY Id);

-- Si pas de catégorie, on en crée 3 et on reprend l'Id de "Mixte"
IF @FallbackCat IS NULL
BEGIN
    INSERT INTO Categories (Nom) VALUES ('Homme');
    INSERT INTO Categories (Nom) VALUES ('Femme');
    INSERT INTO Categories (Nom) VALUES ('Mixte');
    SET @FallbackCat = (SELECT Id FROM Categories WHERE Nom = 'Mixte');
END

-- Mettre à jour les parfums qui ont CategorieId NULL
UPDATE Parfums
SET CategorieId = @FallbackCat
WHERE CategorieId IS NULL;

SELECT COUNT(*) AS ParfumsMisAJour FROM Parfums WHERE CategorieId = @FallbackCat;

