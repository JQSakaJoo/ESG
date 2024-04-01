//+————————————————————————————————————————————————————————————————————————————+
//|                                                                   C_AO_ESG |
//|                                            Copyright 2007-2024, Andrey Dik |
//|                     Copyright 2007-2024, https://www.mql5.com/ru/users/joo |
//—————————————————————————————————————————————————————————————————————————————+
/*
 Алгоритм оптимизации "Эволюция Социальных Групп" (Evolution Of Social Groups, ESG)
 */
public struct S_Group
{
    public double[] cB;
    public double fB;
    public int sSize;
    public double sRadius;

    public void Init(int coords, int groupSize)
    {
        cB    = new double[coords];
        fB    = double.MinValue;
        sSize = groupSize;
    }
}

public struct S_Agent
{
    public double[] c; //coordinates
    public double f;   //fitness

    public void Init(int coords)
    {
        c = new double[coords];
        f = double.MinValue;
    }
}

public class C_AO_ESG
{
    public double[] cB = new double[1];       //best coordinates
    public double fB;                       //FF of the best coordinates
    public S_Agent[] a = new S_Agent[1];      //agents

    public double[] rangeMax = new double[1];
    public double[] rangeMin = new double[1];
    public double[] rangeStep = new double[1];

    private int coords;
    private int popSize;                       //population size
    private S_Group[] gr = new S_Group[1];     //group

    private int groups;                     //number of groups
    private double groupRadius;                //group radius
    private double expansionRatio;             //expansion ratio
    private double power;                      //power

    private bool revision;

    public void Init(int coordinatesNumberP, int populationSizeP, int groupsP, double groupRadiusP, double expansionRatioP, double powerP)
    {
        Random rand = new Random();
        fB = double.MinValue;
        revision = false;

        coords = coordinatesNumberP;
        popSize = populationSizeP;
        groups = groupsP;
        groupRadius = groupRadiusP;
        expansionRatio = expansionRatioP;
        power = powerP;

        int[] partInSwarms = new int[groups];

        int particles = popSize / groups;
        for (int i = 0; i < groups; i++)
        {
            partInSwarms[i] = particles;
        }

        int lost = popSize - particles * groups;

        if (lost > 0)
        {
            int pos = 0;

            while (true)
            {
                partInSwarms[pos]++;
                lost--;
                pos++;
                if (pos >= groups) pos = 0;
                if (lost == 0) break;
            }
        }
        rangeMax = new double[coords];
        rangeMin = new double[coords];
        rangeStep = new double[coords];
        cB = new double[coords];
        gr = new S_Group[groups];

        for (int s = 0; s < groups; s++) gr[s].Init(coords, partInSwarms[s]);

        a = new S_Agent[popSize];
        for (int i = 0; i < popSize; i++) a[i].Init(coords);
    }

    public void Moving()
    {
        if (!revision)
        {
            int cnt = 0;
            double coordinate = 0.0;
            double radius = 0.0;
            double min = 0.0;
            double max = 0.0;

            for (int s = 0; s < groups; s++)
            {
                gr[s].sRadius = groupRadius;

                for (int c = 0; c < coords; c++)
                {
                    coordinate = RNDfromCI(rangeMin[c], rangeMax[c]);

                    gr[s].cB[c] = SeInDiSp(coordinate, rangeMin[c], rangeMax[c], rangeStep[c]);
                }
            }

            for (int s = 0; s < groups; s++)
            {
                for (int p = 0; p < gr[s].sSize; p++)
                {
                    for (int c = 0; c < coords; c++)
                    {
                        radius = (rangeMax[c] - rangeMin[c]) * gr[s].sRadius;
                        min = gr[s].cB[c] - radius;
                        max = gr[s].cB[c] + radius;

                        if (min < rangeMin[c]) min = rangeMin[c];
                        if (max > rangeMax[c]) max = rangeMax[c];

                        coordinate = PowerDistribution(gr[s].cB[c], min, max, power);
                        a[cnt].c[c] = SeInDiSp(coordinate, rangeMin[c], rangeMax[c], rangeStep[c]);
                    }

                    cnt++;
                }
            }

            revision = true;
        }
    }

    public void Revision()
    {
        for (int i = 0; i < popSize; i++)
        {
            if (a[i].f > fB)
            {
                fB = a[i].f;
                Array.Copy(a[i].c, cB, a[i].c.Length);
            }
        }
        int cnt = 0;
        bool impr = false;

        for (int s = 0; s < groups; s++)
        {
            impr = false;

            for (int p = 0; p < gr[s].sSize; p++)
            {
                if (a[cnt].f > gr[s].fB)
                {
                    gr[s].fB = a[cnt].f;
                    Array.Copy(a[cnt].c, gr[s].cB, a[cnt].c.Length);
                    impr = true;
                }
                cnt++;
            }

            if (!impr) gr[s].sRadius *= expansionRatio;
            else gr[s].sRadius = groupRadius;

            if (gr[s].sRadius > 0.5) gr[s].sRadius = 0.5;
        }
        double coordinate = 0.0;
        double radius = 0.0;
        double min = 0.0;
        double max = 0.0;
        cnt = 0;

        for (int s = 0; s < groups; s++)
        {
            for (int p = 0; p < gr[s].sSize; p++)
            {
                for (int c = 0; c < coords; c++)
                {
                    if (RNDfromCI(0.0, 1.0) < 1.0)
                    {
                        radius = (rangeMax[c] - rangeMin[c]) * gr[s].sRadius;
                        min = gr[s].cB[c] - radius;
                        max = gr[s].cB[c] + radius;

                        if (min < rangeMin[c]) min = rangeMin[c];
                        if (max > rangeMax[c]) max = rangeMax[c];

                        coordinate = PowerDistribution(gr[s].cB[c], min, max, power);
                        a[cnt].c[c] = SeInDiSp(coordinate, rangeMin[c], rangeMax[c], rangeStep[c]);
                    }
                }
                cnt++;
            }
        }
        cnt = 0;

        for (int s = 0; s < groups; s++)
        {
            for (int c = 0; c < coords; c++)
            {
                int posSw = (int)RNDfromCI(0, groups);
                if (posSw >= groups) posSw = groups - 1;

                a[cnt].c[c] = gr[posSw].cB[c];
            }
            cnt += gr[s].sSize;
        }
    }

    private double SeInDiSp(double In, double InMin, double InMax, double Step)
    {
        if (In <= InMin) return (InMin);
        if (In >= InMax) return (InMax);
        if (Step == 0.0) return (In);
        else return (InMin + Step * Math.Round((In - InMin) / Step));
    }

    private double RNDfromCI(double min, double max)
    {
        if (min == max) return (min);
        double Min, Max;
        if (min > max)
        {
            Min = max;
            Max = min;
        }
        else
        {
            Min = min;
            Max = max;
        }
        Random rand = new Random();
        return (double)(Min + ((Max - Min) * rand.NextDouble()));
    }

    private double Scale(double In, double InMIN, double InMAX, double OutMIN, double OutMAX, bool revers)
    {
        if (OutMIN == OutMAX) return (OutMIN);
        if (InMIN == InMAX) return (double)((OutMIN + OutMAX) / 2.0);
        else
        {
            if (In < InMIN) return revers ? OutMAX : OutMIN;
            if (In > InMAX) return revers ? OutMIN : OutMAX;
            double res = (((In - InMIN) * (OutMAX - OutMIN) / (InMAX - InMIN)) + OutMIN);
            if (!revers) return res;
            else return OutMAX - res;
        }
    }

    private double PowerDistribution(double In, double outMin, double outMax, double p)
    {
        double rnd = RNDfromCI(-1.0, 1.0);
        double r = Math.Pow(Math.Abs(rnd), p);

        if (rnd >= 0.0) return In + Scale(r, 0.0, 1.0, 0.0, outMax - In, false);
        else return In - Scale(r, 0.0, 1.0, 0.0, In - outMin, false);
    }
}