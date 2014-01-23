// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using TinyMoneyManager.Data.Model;

namespace PhoneToolkitSample.Data
{
    public class PeopleByFirstName : List<PeopleInGroup>
    {
        private static readonly string Groups = "#abcdefghijklmnopqrstuvwxyz";

        private Dictionary<int, PeopleProfile> _personLookup = new Dictionary<int, PeopleProfile>();

        public PeopleByFirstName()
        {
            List<PeopleProfile> people = new List<PeopleProfile>(AllPeople.Current);
            people.Sort(PeopleProfile.CompareByFirstName);

            Dictionary<string, PeopleInGroup> groups = new Dictionary<string, PeopleInGroup>();

            foreach (char c in Groups)
            {
                PeopleInGroup group = new PeopleInGroup(c.ToString());
                this.Add(group);
                groups[c.ToString()] = group;
            }
            PeopleProfile person1 = null;
            try
            {
                foreach (PeopleProfile person in people)
                {
                    person1 = person;
                    groups[PeopleProfile.GetFirstNameKey(person)].Add(person);
                }
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
