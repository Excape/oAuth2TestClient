CREATE TABLE dbo.Director (
	[DirectorId]	INT		IDENTITY(1, 1) NOT NULL,
	[Firstname]		NVARCHAR(40)			NULL,
	[Lastname]		NVARCHAR(40)			NULL,
	PRIMARY KEY ([DirectorId] ASC)
);

CREATE TABLE dbo.Movie (
	[MovieId]	INT		IDENTITY(1, 1) NOT NULL,
	[Title]		NVARCHAR(60)			NULL,
	[DirectorId]	INT					NULL,
	PRIMARY KEY ([MovieId] ASC),
	CONSTRAINT [FK_dbo.Movie_dbo.Director_DirectorId] FOREIGN KEY ([DirectorId])
		REFERENCES [Director] ([DirectorId])
);

INSERT INTO dbo.Director (Firstname, Lastname) VALUES
	('Christopher', 'Nolan'),
	('Stanley', 'Kubrick'),
	('Steven', 'Spielberg'),
	('Rian', 'Johnson'),
	('Martin', 'Scorsese'),
	('Darren', 'Aronofsky'),
	('Lars', 'von Trier')
;

INSERT INTO dbo.Movie (Title, DirectorId) VALUES
	('The Dark Knight', 1),
	('A Clockwork Orange', 2),
	('Jurassic Park', 3),
	('Looper', 4),
	('The Wolf of Wall Street', 5),
	('Requiem For a Dream', 6),
	('Inception', 1)
;
