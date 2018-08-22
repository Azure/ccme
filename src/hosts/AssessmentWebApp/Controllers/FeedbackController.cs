using Microsoft.Azure.CCME.Assessment.Hosts.Controllers;
using System.Web.Mvc;

namespace Microsoft.Azure.Mrm.Assessment.Hosts.Controllers
{
    public class FeedbackController : BaseController
    {
        public ActionResult Index()
        {
            return this.View();
        }
    }
}