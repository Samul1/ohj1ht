using Jypeli;
using System;
using System.Collections.Generic;
using System.Text;


public class Zombi : PhysicsObject
{
    public int Osumat; // Muuta private, kun zombien tappaminen on korjattu!
    private Color[] Varit;
    public Zombi(double leveys, double korkeus, Color[] varit) : base(leveys, korkeus)
    {
        this.Color = varit[0];
        Osumat = 0;
        Varit = varit;
    }

    public void OtaVastaanOsuma()
    {
        Osumat++;
        if(Osumat >= Varit.Length)
        {
            TuhoaZombi();
            return;
        }
        this.Color = Varit[Osumat];
    }

    public virtual void TuhoaZombi()
    {
        this.Destroy();
        Osumat = 0;
    }

}

