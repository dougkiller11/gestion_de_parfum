-- À exécuter dans la base GestionParfum
USE GestionParfum;

DECLARE @Nom NVARCHAR(100) = 'Admin';
DECLARE @Email NVARCHAR(100) = 'admin@admin.com';
DECLARE @Pwd NVARCHAR(100) = 'admin';

-- Crée l'utilisateur si absent
IF NOT EXISTS (SELECT 1 FROM Utilisateurs WHERE Email = @Email)
BEGIN
    INSERT INTO Utilisateurs (Nom, Email, MotDePasse)
    VALUES (@Nom, @Email, @Pwd);
END

DECLARE @UserId INT = (SELECT TOP 1 Id FROM Utilisateurs WHERE Email = @Email);

-- Crée l'admin si absent
IF NOT EXISTS (SELECT 1 FROM Administrateur WHERE Id = @UserId)
BEGIN
    INSERT INTO Administrateur (Id) VALUES (@UserId);
END

SELECT 'Admin ready with email=' + @Email + ' / pwd=' + @Pwd AS Info;

