using ByzantineGenerals.PowBlockchain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByzantineGenerals.Pow.Tests
{
    [TestClass]
    public class CommandServiceTests
    {
        [TestMethod]
        public void AddGenerals()
        {
            CommandService commandService = new CommandService();
            General general = commandService.CreateGeneral(Decisions.Attack);

            Assert.AreEqual(1, commandService.GetAllGenerals().Count);
        }

        [TestMethod]
        public void AddGenerals2()
        {
            CommandService commandService = new CommandService();
            General general = commandService.CreateGeneral(Decisions.Attack);

            Assert.AreEqual(1, commandService.GetAllGenerals().Count);
        }
    }
}
