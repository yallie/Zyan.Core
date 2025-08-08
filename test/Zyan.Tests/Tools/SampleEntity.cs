using System;
using System.Collections.Generic;

namespace Zyan.Tests.Tools;

/// <summary>
/// Sample entity class for Linq tests
/// </summary>
[Serializable]
public class SampleEntity
{
    public SampleEntity()
    {
        Id = -1;
        FirstName = "<Noname>";
        LastName = "<Unknown>";
    }

    public SampleEntity(int id, string name, string lastName)
    {
        Id = id;
        FirstName = name;
        LastName = lastName;
    }

    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    static public IList<SampleEntity> GetSampleEntities() =>
    [
        new(1, "Albert", "Einstein"),
        new(2, "Niels", "Bohr"),
        new(3, "Ralph", "Alpher"),
        new(4, "Hans", "Bethe"),
        new(5, "George", "Gamow"),
        new(6, "Alexander", "Friedmann"),
        new(7, "Enrico", "Fermi"),
        new(8, "Richard", "Feynman"),
        new(9, "Lev", "Landau"),
        new(10, "Pyotr", "Kapitsa"),
        new(11, "Robert", "Oppenheimer"),
        new(12, "James", "Chadwick"),
        new(13, "Arthur", "Compton"),
        new(14, "Klaus", "Fuchs"),
        new(15, "William", "Penney"),
        new(16, "Emilio", "Segrè"),
        new(17, "Ernest", "Lawrence"),
        new(18, "Glenn", "Seaborg"),
        new(19, "Leó", "Szilárd"),
        new(20, "Edward", "Teller"),
        new(21, "Stanislaw", "Ulam"),
        new(22, "Harold", "Urey"),
        new(23, "Leona", "Woods"),
        new(24, "Chien-Shiung", "Wu"),
        new(25, "Robert", "Wilson"),
        new(26, "Igor", "Kurchatov")
    ];
}
