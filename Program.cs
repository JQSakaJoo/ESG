public class Program
{
    public static double Core(double x, double y)
    {
        double res = 20.0 + x * x + y * y
                     - 10.0 * Math.Cos(2.0 * Math.PI * x)
                     - 10.0 * Math.Cos(2.0 * Math.PI * y);

        return Scale(res, 1.2390563437933917e-14, 80.70658038767792, 0.0, 1.0);
    }

    public static double Scale(double In, double InMIN, double InMAX, double OutMIN, double OutMAX)
    {
        if (OutMIN == OutMAX) return (OutMIN);
        if (InMIN == InMAX) return (double)((OutMIN + OutMAX) / 2.0);
        else
        {
            if (In < InMIN) return OutMIN;
            if (In > InMAX) return OutMAX;
            double res = (((In - InMIN) * (OutMAX - OutMIN) / (InMAX - InMIN)) + OutMIN);
            return res;
        }
    }

    public static void Main()
    {
        // Создаем экземпляр класса C_AO_ESG
        var esg = new C_AO_ESG();

        // Инициализируем алгоритм с заданными параметрами
        esg.Init(2, 200, 100, 0.1, 2.0, 10);

        esg.rangeMax[0] = 5;
        esg.rangeMax[1] = 5;
        esg.rangeMin[0] = -5;
        esg.rangeMin[1] = -5;
        esg.rangeStep[0] = 0;
        esg.rangeStep[1] = 0;

        for (int i = 0; i < 100; i++) 
        {
            esg.Moving();

            for (int p = 0; p < esg.a.Length; p++)
            {
                esg.a[p].f = Core (esg.a[p].c[0], esg.a[p].c[1]); 
            }

            esg.Revision();

            Console.WriteLine(esg.fB.ToString() + " " + esg.cB[0] + " " + esg.cB[1]);
        }

        Console.WriteLine(esg.fB.ToString() + " " + esg.cB[0] + " " + esg.cB[1]);
        
        Console.ReadLine();
    }
}