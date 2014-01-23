// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Generic;
using TinyMoneyManager.Data.Model;
using System;

namespace PhoneToolkitSample.Data
{
    public class AllPeople : IEnumerable<PeopleProfile>
    {
        private static Dictionary<int, PeopleProfile> _personLookup;
        private static AllPeople _instance;

        public static AllPeople Current
        {
            get
            {
                return _instance ?? (_instance = new AllPeople());
            }
        }

        public PeopleProfile this[int index]
        {
            get
            {
                PeopleProfile person;
                _personLookup.TryGetValue(index, out person);
                return person;
            }
        }

        #region IEnumerable<PeopleProfile> Members

        public IEnumerator<PeopleProfile> GetEnumerator()
        {
            EnsureData();
            return _personLookup.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            EnsureData();
            return _personLookup.Values.GetEnumerator();
        }

        #endregion

        private void EnsureData()
        {
            if (_personLookup == null)
            {
                _personLookup = AllPeopleGetter();
            }
        }

        public
            static Func<Dictionary<int, PeopleProfile>> AllPeopleGetter;


        public bool HasLoaded { get; set; }
    }
}
