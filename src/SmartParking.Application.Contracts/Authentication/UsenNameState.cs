using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SmartParking.Authentication;
public class UserNameState : IScopedDependency
{
    public string UserName { get; set; }
}
