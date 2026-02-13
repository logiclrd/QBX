namespace QBX.OperatingSystem.FileStructures;

public enum FileMode
{
	CreateNew = System.IO.FileMode.CreateNew,
	Create = System.IO.FileMode.Create,
	Open = System.IO.FileMode.Open,
	OpenOrCreate = System.IO.FileMode.OpenOrCreate,
	Truncate = System.IO.FileMode.Truncate,
	Append = System.IO.FileMode.Append,
}
