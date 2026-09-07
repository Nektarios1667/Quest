using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Entities;

public interface IHasDialog
{
    public string GetFullDialog();
    public string GetName();
}
