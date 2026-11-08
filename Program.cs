
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Клас замовлення
class Order
{
    public int Number { get; set; }
    public string Description { get; set; } = "";
    public double Weight { get; set; }
    public double Distance { get; set; }
    public string Status { get; set; } = "Активне";
}

// Абстрактний клас транспорту
abstract class Transport
{
    public string Name { get; set; }
    public bool IsBusy { get; set; }

    public Transport(string name)
    {
        Name = name;
        IsBusy = false;
    }

    public abstract double CalculateCost(Order order);

    public virtual bool CanTake(Order order)
    {
        return order.Weight > 0 && order.Distance > 0;
    }
}

// Велосипед
class Bike : Transport
{
    public Bike() : base("Велосипед")
    {
    }

    public override double CalculateCost(Order order)
    {
        return 20 + 5 * order.Distance;
    }

    public override bool CanTake(Order order)
    {
        return base.CanTake(order) && order.Distance <= 10;
    }
}

// Автомобіль
class Car : Transport
{
    public Car() : base("Автомобіль")
    {
    }

    public override double CalculateCost(Order order)
    {
        return 50 + 8 * order.Distance + order.Weight;
    }
}

// Вантажівка
class Truck : Transport
{
    public Truck() : base("Вантажівка")
    {
    }

    public override double CalculateCost(Order order)
    {
        return 200 + 15 * order.Distance + 0.5 * order.Weight;
    }
}

// Інтерфейс оплати
interface IPayment
{
    void Pay(double amount);
}

// Оплата карткою
class CardPayment : IPayment
{
    public void Pay(double amount)
    {
        Console.WriteLine($"Оплачено карткою: {amount:F2} грн");
    }
}

// Оплата готівкою
class CashPayment : IPayment
{
    public void Pay(double amount)
    {
        Console.WriteLine($"Оплачено готівкою: {amount:F2} грн");
    }
}

// Клас служби доставки
class DeliveryService
{
    private List<Order> orders = new List<Order>();
    private List<Order> history = new List<Order>();

    private List<Transport> transports = new List<Transport>();

    private Dictionary<int, Transport> assigned =
        new Dictionary<int, Transport>();

    public DeliveryService()
    {
        transports.Add(new Bike());
        transports.Add(new Car());
        transports.Add(new Truck());
    }

    // Створення замовлення
    public void CreateOrder(int number, string description,
                            double weight, double distance)
    {
        if (number <= 0)
        {
            Console.WriteLine("Номер повинен бути більшим за 0!");
            return;
        }

        foreach (Order order in orders)
        {
            if (order.Number == number)
            {
                Console.WriteLine("Замовлення з таким номером вже існує!");
                return;
            }
        }

        foreach (Order order in history)
        {
            if (order.Number == number)
            {
                Console.WriteLine("Цей номер вже є в історії!");
                return;
            }
        }

        if (weight <= 0 || distance <= 0 ||
            double.IsNaN(weight) || double.IsNaN(distance) ||
            double.IsInfinity(weight) || double.IsInfinity(distance))
        {
            Console.WriteLine("Введіть правильну додатну вагу та відстань!");
            return;
        }

        Order newOrder = new Order
        {
            Number = number,
            Description = description,
            Weight = weight,
            Distance = distance
        };

        orders.Add(newOrder);

        Console.WriteLine("Замовлення успішно створено!");
    }

    // Вартість доставки
    public void ShowCosts(int number)
    {
        Order order = orders.Find(o => o.Number == number);

        if (order == null)
        {
            Console.WriteLine("Замовлення не знайдено!");
            return;
        }

        Console.WriteLine("\nВартість доставки:");

        foreach (Transport transport in transports)
        {
            if (transport.CanTake(order))
            {
                double cost = transport.CalculateCost(order);

                Console.WriteLine(
                    $"{transport.Name}: {cost:F2} грн");
            }
            else
            {
                Console.WriteLine(
                    $"{transport.Name}: не може доставити");
            }
        }
    }

    // Призначення транспорту
    public void AssignTransport(int number, int transportNumber)
    {
        Order order = orders.Find(o => o.Number == number);

        if (order == null)
        {
            Console.WriteLine("Замовлення не знайдено!");
            return;
        }

        if (assigned.ContainsKey(number))
        {
            Console.WriteLine("Замовлення вже призначене!");
            return;
        }

        if (transportNumber < 1 ||
            transportNumber > transports.Count)
        {
            Console.WriteLine("Неправильний номер транспорту!");
            return;
        }

        Transport transport = transports[transportNumber - 1];

        if (transport.IsBusy)
        {
            Console.WriteLine("Транспорт зараз зайнятий!");
            return;
        }

        if (!transport.CanTake(order))
        {
            Console.WriteLine("Транспорт не може взяти замовлення!");
            return;
        }

        transport.IsBusy = true;
        order.Status = "В дорозі";

        assigned.Add(number, transport);

        Console.WriteLine(
            $"Замовлення призначено на транспорт: {transport.Name}");
    }

    // Доставка та оплата
    public void DeliverOrder(int number, int paymentType)
    {
        Order order = orders.Find(o => o.Number == number);

        if (order == null)
        {
            Console.WriteLine("Замовлення не знайдено!");
            return;
        }

        if (!assigned.ContainsKey(number))
        {
            Console.WriteLine("Спочатку призначте транспорт!");
            return;
        }

        if (paymentType != 1 && paymentType != 2)
        {
            Console.WriteLine("Неправильний спосіб оплати!");
            return;
        }

        Transport transport = assigned[number];

        double cost = transport.CalculateCost(order);

        IPayment payment;

        if (paymentType == 1)
        {
            payment = new CardPayment();
        }
        else
        {
            payment = new CashPayment();
        }

        payment.Pay(cost);

        transport.IsBusy = false;
        order.Status = "Доставлено";

        history.Add(order);
        orders.Remove(order);
        assigned.Remove(number);

        Console.WriteLine("Замовлення успішно доставлено!");
    }

    // Активні замовлення
    public void ShowOrders()
    {
        Console.WriteLine("\nАКТИВНІ ЗАМОВЛЕННЯ:");

        if (orders.Count == 0)
        {
            Console.WriteLine("Активних замовлень немає.");
            return;
        }

        foreach (Order order in orders)
        {
            Console.WriteLine(
                $"№{order.Number} | {order.Description} | " +
                $"{order.Weight} кг | {order.Distance} км | " +
                $"{order.Status}");
        }
    }

    // Історія доставок
    public void ShowHistory()
    {
        Console.WriteLine("\nІСТОРІЯ ДОСТАВОК:");

        if (history.Count == 0)
        {
            Console.WriteLine("Історія порожня.");
            return;
        }

        foreach (Order order in history)
        {
            Console.WriteLine(
                $"№{order.Number} | {order.Description} | " +
                $"{order.Status}");
        }
    }

    // Стан транспорту
    public void ShowTransports()
    {
        Console.WriteLine("\nТРАНСПОРТ:");

        for (int i = 0; i < transports.Count; i++)
        {
            string status = transports[i].IsBusy
                ? "Зайнятий"
                : "Вільний";

            Console.WriteLine(
                $"{i + 1}. {transports[i].Name} - {status}");
        }
    }
}

// Головний клас
class Program
{
    // Введення цілого числа
    static int ReadInt(string message)
    {
        int number;

        while (true)
        {
            Console.Write(message);

            if (int.TryParse(Console.ReadLine(), out number))
            {
                return number;
            }

            Console.WriteLine("Помилка! Введіть ціле число.");
        }
    }

    // Введення дробового числа
    static double ReadDouble(string message)
    {
        double number;

        while (true)
        {
            Console.Write(message);

            string input = Console.ReadLine() ?? "";
            input = input.Replace('.', ',');

            if (double.TryParse(
                input,
                NumberStyles.Float,
                CultureInfo.GetCultureInfo("uk-UA"),
                out number) &&
                !double.IsNaN(number) &&
                !double.IsInfinity(number))
            {
                return number;
            }

            Console.WriteLine("Помилка! Введіть правильне число.");
        }
    }

    // Головний метод програми
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        DeliveryService service = new DeliveryService();

        int choice;

        do
        {
            // Очищення екрана перед меню
            Console.Clear();

            Console.WriteLine("==============================");
            Console.WriteLine("       СЛУЖБА ДОСТАВКИ");
            Console.WriteLine("==============================");

            Console.WriteLine("1. Створити замовлення");
            Console.WriteLine("2. Показати вартість доставки");
            Console.WriteLine("3. Призначити транспорт");
            Console.WriteLine("4. Доставити замовлення");
            Console.WriteLine("5. Активні замовлення");
            Console.WriteLine("6. Історія доставок");
            Console.WriteLine("7. Стан транспорту");
            Console.WriteLine("0. Вихід");

            choice = ReadInt("\nОберіть пункт: ");

            // Очищення головного меню
            Console.Clear();

            switch (choice)
            {
                case 1:
                    {
                        Console.WriteLine("СТВОРЕННЯ ЗАМОВЛЕННЯ\n");

                        int number = ReadInt("Номер замовлення: ");

                        Console.Write("Опис: ");
                        string description = Console.ReadLine() ?? "";

                        double weight = ReadDouble("Вага (кг): ");
                        double distance = ReadDouble("Відстань (км): ");

                        service.CreateOrder(
                            number, description, weight, distance);

                        break;
                    }

                case 2:
                    {
                        Console.WriteLine("ВАРТІСТЬ ДОСТАВКИ\n");

                        int number = ReadInt("Номер замовлення: ");

                        service.ShowCosts(number);

                        break;
                    }

                case 3:
                    {
                        Console.WriteLine("ПРИЗНАЧЕННЯ ТРАНСПОРТУ\n");

                        int number = ReadInt("Номер замовлення: ");

                        service.ShowTransports();

                        int transport = ReadInt(
                            "\nОберіть транспорт (1-3): ");

                        service.AssignTransport(number, transport);

                        break;
                    }

                case 4:
                    {
                        Console.WriteLine("ДОСТАВКА ЗАМОВЛЕННЯ\n");

                        int number = ReadInt("Номер замовлення: ");

                        Console.WriteLine("1. Карткою");
                        Console.WriteLine("2. Готівкою");

                        int payment = ReadInt("Спосіб оплати: ");

                        service.DeliverOrder(number, payment);

                        break;
                    }

                case 5:
                    service.ShowOrders();
                    break;

                case 6:
                    service.ShowHistory();
                    break;

                case 7:
                    service.ShowTransports();
                    break;

                case 0:
                    Console.WriteLine("Завершення програми...");
                    break;

                default:
                    Console.WriteLine("Невідомий пункт меню!");
                    break;
            }

            // Очікування перед поверненням
            if (choice != 0)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "Натисніть Enter, щоб повернутися до головного меню...");

                Console.ReadLine();
            }

        } while (choice != 0);
    }
}
