using System;

namespace Arad.Portal.GeneralLibrary.CustomAttributes;

public class CustomCollectionNameAttribute : Attribute
{
    /// <summary>
    /// The name of the collection in which your documents are stored.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="name">The name of the collection.</param>
    public CustomCollectionNameAttribute(string name)
    {
        Name = name;
    }
}