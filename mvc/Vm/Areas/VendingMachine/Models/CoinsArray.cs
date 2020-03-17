namespace Vm.Areas.VendingMachine.Models
{
	/// <summary>
	/// Класс "Набор монет" (несколько монет одного достоинства)
	/// </summary>
    public class CoinsArray
    {
	    public int Value { get; set; }
	    public int Count { get; set; }


	    public CoinsArray(int value, int count)
	    {
	        this.Value = value;
	        this.Count = count;
	    }

    
        //// Общая стоимость набора монет
        //self.sum = ko.computed(function()
        //{
        //    var value = self.value();
        //    var count = self.count();
        //    return value*count;
        //}

	    public override string ToString()
	    {
	        return string.Format("{0} руб - {1} шт", Value, Count);
	    }
    }
}