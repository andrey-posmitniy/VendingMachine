namespace Vm.Areas.VendingMachine.Models
{
	/// <summary>
	/// Класс "Набор товаров"
	/// </summary>
    public class ProductsArray
    {
        public int Id { get; set; }
        public string Name { get; set; }
	    public int Price { get; set; }
	    public int Count { get; set; }


	    public ProductsArray(int id, string name, int price, int count)
	    {
            this.Id = id;
            this.Name = name;
            this.Price = price;
            this.Count = count;
	    }


	    public override string ToString()
	    {
	        return string.Format("{0} - {1} руб - {2} шт", Name, Price, Count);
	    }
    }
}