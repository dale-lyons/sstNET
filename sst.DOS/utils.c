#include "sst.h"

//static int _peekRand = 0;
//static double _peek = 0;
static int _randomSeed = 0;
int _nextCOMTable = 1;
int _nextKLTable = 1;

void SetRandomSeed(int seed)
{
	_randomSeed = seed;
}

int GetRandomSeed()
{
	return _randomSeed;
}

double PeekRand()
{
	double dran;
	int ran;
	int seed = _randomSeed * 0x343FD + 0x269EC3;
    ran = ((seed >> 0x10) & 0x7FFF);
    dran = (double)ran / (1.0 + (double)32767);
    return dran;
}

double Rand(void)
{
	double dran;
	int ran;
    _randomSeed = _randomSeed * 0x343FD + 0x269EC3;
    ran = ((_randomSeed >> 0x10) & 0x7FFF);
    dran = (double)ran / (1.0 + (double)32767);
    return dran;
}





/*
static int _peekRand = 0;
static double _peek = 0.0;
int _lastRand = 0;
double PeekRand()
{
	if(_peekRand)
	{
		return _peek;
	}
	else
	{
		_peek = Rand();
		_peekRand = 1;
		return _peek;
	}
}
double Rand(void)
{
	double value;
	if(_peekRand)
	{
		_peekRand = 0;
		return _peek;
	}
	else
	{
		_lastRand = rand();
		value = _lastRand;
		value = value /(1.0 + (double)RAND_MAX);
		return value;
		//return rand()/(1.0 + (double)RAND_MAX);
	}
}
*/

void dumpkl()
{
	int ii,id;
	int num = nenhere;

	if(!isdebug())
		return;

	id = _nextKLTable++;

	printf("===Enemy Table:%d ===\n",id);
	//printf("    x, y,  dist,  avdist,  power,  kid\n");
	for(ii=1; ii<=num; ii++)
	{
		//printf("%4d:%4d:%4d,%4d:%8.2f,%8.2f,%8.2f\n",ii,kid[ii],kx[ii],ky[ii],kdist[ii],kavgd[ii],kpower[ii]);
		printf("%4d:%4d,%4d:%8.2f,%8.2f,%8.2f\n",ii,kx[ii],ky[ii],kdist[ii],kavgd[ii],kpower[ii]);
	}
	printf("===Enemy Table:%d ===\n",id);
	printf("Next Random:%2.8f\n",PeekRand());
	if(d.isx != 0 && d.isy != 0)
		printf("SuperCommander is at:%d-%d\n",d.isx,d.isy);

	if(thingx!=0)
		printf("Thing is at X:%2d,Y:%2d\n", thingx, thingy);
}

void dumpRandom()
{
	if(!isdebug())
		return;

	printf("Next Random:%2.8f\n",PeekRand());
}

/*compress the klingon table.
  A 0 in the kx field indicates empty.*/
void collapsekl()
{
	int removed;
	removed = 1;
	while(removed)
	{
		int ii, jj, num;
		removed = 0;
		num = nenhere;
		for(ii=1; ii<=num; ii++)
		{
			if(kx[ii]==0)
			{
				for(jj=ii; jj<num; jj++)
				{
					kx[jj]=kx[jj+1];
					ky[jj]=ky[jj+1];
					kavgd[jj]=kavgd[jj+1];
					kpower[jj]=kpower[jj+1];
					kdist[jj]=kdist[jj+1];
					kid[jj]=kid[jj+1];
				}/*for jj*/

				kx[num]=0;
				ky[num]=0;
				kavgd[num]=0;
				kpower[num]=0;
				kdist[num]=0;
				kid[num]=0;
				nenhere--;

				removed = 1;
				break;
			}/*if*/
		}/*for ii*/
	}/*while*/
}

void dumpfuture()
{
	int l;

		if(!isdebug())
		return;

	prout("====FUTURE EVENTS====");
	for (l=1; l<=NEVENTS; l++)
	{
		printf("%d:%8.2f\n",l,future[l]);
	}
	printf("Next Random:%2.8f\n",PeekRand());
	prout("====FUTURE EVENTS====");
}

void dumpcom()
{
	int ii;
	int num = d.remcom;
	int id;

	if(!isdebug())
		return;

	id = _nextCOMTable++;

	printf("===Enemy Commanders Table:%d ===\n",id);
	//printf("    x, y,  dist,  avdist,  power,  kid\n");
	for(ii=1; ii<=num; ii++)
	{
		printf("%4d:%4d,%4d\n",d.cid[ii],d.cx[ii],d.cy[ii]);
	}
	printf("===Enemy Commanders Table:%d ===\n",id);
	//printf("Next Random:%2.8f\n",PeekRand());
	//if(d.isx != 0 && d.isy != 0)
	//	printf("SuperCommander is at:%d-%d\n",d.isx,d.isy);
}

void sortcom()
{
	int sw, k;
	// The author liked bubble sort. So we will use it. :-(
	if (d.remcom < 2) return;

	do {
		int j;
		sw = 0;
		for (j = 1; j < d.remcom; j++)
		{
			if(d.cid[j] > d.cid[j+1])
			{
				sw = 1;
				k = d.cx[j];
				d.cx[j] = d.cx[j+1];
				d.cx[j+1]=k;

				k = d.cy[j];
				d.cy[j] = d.cy[j+1];
				d.cy[j+1]=k;

				k = d.cid[j];
				d.cid[j] = d.cid[j+1];
				d.cid[j+1]=k;
			}
		}
	} while (sw);
}

void dumpsnap(double date1, double date2)
{
	if(!isdebug())
		return;

	printf("Snapshot date:%.2f\n", date1);
	printf("Snapshot next:%.2f\n", date2);

}

void sortbase()
{
	int sw, k;
	// The author liked bubble sort. So we will use it. :-(
	if (d.rembase < 2) return;

	do {
		int j;
		sw = 0;
		for (j = 1; j < d.rembase; j++)
		{
			if(d.baseid[j] > d.baseid[j+1])
			{
				sw = 1;
				k = d.baseqx[j];
				d.baseqx[j] = d.baseqx[j+1];
				d.baseqx[j+1]=k;

				k = d.baseqy[j];
				d.baseqy[j] = d.baseqy[j+1];
				d.baseqy[j+1]=k;

				k = d.baseid[j];
				d.baseid[j] = d.baseid[j+1];
				d.baseid[j+1]=k;
			}
		}
	} while (sw);
}

void dumpbase()
{
	int ii;
	int num = d.rembase;

	if(!isdebug())
		return;

	printf("========= Starbases =========\n");
	for(ii=1; ii<=num; ii++)
	{
		printf("ID:%d X:%d Y:%d\n", d.baseid[ii], d.baseqx[ii], d.baseqy[ii]);
	}
	printf("========= Starbases =========\n");
}
