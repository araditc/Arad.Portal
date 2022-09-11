using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arad.Portal.DataLayer.Models.Shared
{
    public class SuggestionObjectComparer : IEqualityComparer<SuggestionObject>
    {
         // SuggestionObjects are equal if their phrase and url are equal.
        public bool Equals(SuggestionObject x, SuggestionObject y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

        //Check whether SuggestionObject properties are equal.
        return x.Phrase == y.Phrase && x.Url == y.Url;
        }

    // If Equals() returns true for a pair of objects
    // then GetHashCode() must return the same value for these objects.

    public int GetHashCode(SuggestionObject suggestionObject)
    {
        //Check whether the object is null
        if (Object.ReferenceEquals(suggestionObject, null)) return 0;

        //Get hash code for the phrase field if it is not null.
        int hashPhrase = suggestionObject.Phrase == null ? 0 : suggestionObject.Phrase.GetHashCode();

        //Get hash code for the Url field.
        int hashUrl = suggestionObject.Url.GetHashCode();

        //Calculate the hash code for the product.
        return hashPhrase ^ hashUrl;
    }
}
}
