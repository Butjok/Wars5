#define MIN(a,b) (((a)<(b))?(a):(b))
#define INDEX(i,j) ((i)*count + (j))

void calculate(int count, int* distances){
    for (int k = 0; k < count; k++)
        for (int i = 0; i < count; i++)
            for (int j = 0; j < count; j++)
                distances[INDEX(i,j)] = MIN(distances[INDEX(i,j)], distances[INDEX(i,k)] + distances[INDEX(k,j)]);
}
