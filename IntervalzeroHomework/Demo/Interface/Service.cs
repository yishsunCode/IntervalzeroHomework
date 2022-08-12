using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Interface
{
    public interface IInputService 
    {
        IList<string> GetArticleList();
        string GetArticle(string articleName);
    }
}
