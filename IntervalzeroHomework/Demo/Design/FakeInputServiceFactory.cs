using Demo.Interface;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Design
{
    static class FakeInputServiceFactory
    {
        public static IInputService Create() 
        {
            var serv = Substitute.For<IInputService>();
            var articleList = Enumerable.Range(0, 10).Select(i => $"ArticleFromServ_{i}").ToArray();
            serv.GetArticleList().ReturnsForAnyArgs(articleList);
            var text = "Hello World !";
            serv.GetArticle(default).ReturnsForAnyArgs(text);

            return serv;
        }
    }
}
