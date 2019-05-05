using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xamarin.Auth;

namespace xnano.Models
{
    public interface IAccountStorage
    {
        Task SaveAsync(Account account);
        Task<List<Account>> FindAccountsForServiceAsync();
    }
}