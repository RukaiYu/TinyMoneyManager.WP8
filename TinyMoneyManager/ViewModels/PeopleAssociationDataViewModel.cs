namespace TinyMoneyManager.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using TinyMoneyManager.Component;
    using TinyMoneyManager.Data.Model;

    public class PeopleAssociationDataViewModel : NkjSoftViewModelBase
    {
        public System.Collections.Generic.IEnumerable<PeopleAssociationData> QueryPeoplesForAttachedId(System.Guid attachedId, System.Action<PeopleAssociationData> itemCallback = null)
        {
            IQueryable<PeopleAssociationData> queryable = from p in this.AccountBookDataContext.PeopleAssociationDatas
                                                          where p.AttachedId == attachedId
                                                          select p;
            if (itemCallback != null)
            {
                foreach (PeopleAssociationData data in queryable)
                {
                    itemCallback(data);
                }
            }
            return queryable;
        }
    }
}

