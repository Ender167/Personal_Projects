
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include <cmath>
#include <iostream>
#include <fstream>
#include <string>
#include <chrono>
#include <omp.h>

using namespace std;
using namespace chrono;

__global__ void propagate(
    float* pos_x, float* pos_y, float* pos_z,
    float* vel_x, float* vel_y, float* vel_z,
    float* out_x, float* out_y, float* out_z,
    float dt, float mu, int N, int steps, int output_interval)
{
    int i = blockIdx.x * blockDim.x + threadIdx.x;
    if (i >= N) return;

    float x = pos_x[i];
    float y = pos_y[i];
    float z = pos_z[i];

    float vx = vel_x[i];
    float vy = vel_y[i];
    float vz = vel_z[i];

    int out_idx = 0;

    for (int t = 0; t < steps; t++) {

        float r2 = x * x + y * y + z * z;

        float inv_r3 = rsqrtf(r2);
        inv_r3 = inv_r3 * inv_r3 * inv_r3;
        float factor = -mu * inv_r3;

        float ax = factor * x; //-mu * x * inv_r3;
        float ay = factor * y;//-mu * y * inv_r3;
        float az = factor * z; // -mu * z * inv_r3;

        vx += ax * dt;
        vy += ay * dt;
        vz += az * dt;

        x += vx * dt;
        y += vy * dt;
        z += vz * dt;

        if (t % output_interval == 0) {
            int idx = out_idx * N + i;
            out_x[idx] = x;
            out_y[idx] = y;
            out_z[idx] = z;
            out_idx++;
        }
    }

    pos_x[i] = x;
    pos_y[i] = y;
    pos_z[i] = z;

    vel_x[i] = vx;
    vel_y[i] = vy;
    vel_z[i] = vz;
}
void write_positions(int t, int N, float* pos_x, float* pos_y, float* pos_z) {
    string fileName = "frames\\frame_" + to_string(t) + ".csv";
    ofstream file(fileName);
    for (int i = 0; i < N; i++) {
        file << pos_x[i] << "," << pos_y[i] << "," << pos_z[i] << "\n";
    }
    file.close();
}
int main()
{
    int N = 10;
    float dt = 1.0f; // seconds
    float mu = 3.986e5f; // km^3/s^2
    int steps = 1000;
    int output_interval = 10;

    float R_earth = 6371.0f;  // km
    float altitude = 400.0f;  // km
    float r_mag = R_earth + altitude;
    float v_circ = sqrt(mu / r_mag);

    int num_outputs = (steps - 1) / output_interval + 1;


    float* h_pos_x = new float[N];
    float* h_pos_y = new float[N];
    float* h_pos_z = new float[N];

    float* h_vel_x = new float[N];
    float* h_vel_y = new float[N];
    float* h_vel_z = new float[N];

    float* h_out_x = new float[num_outputs * N];
    float* h_out_y = new float[num_outputs * N];
    float* h_out_z = new float[num_outputs * N];



    for (int i = 0; i < N; i++) {
        float angle = 2.0f * 3.14159265f * i / N;
        h_pos_x[i] = r_mag * cosf(angle);
        h_pos_y[i] = r_mag * sinf(angle);
        h_pos_z[i] = 0.0f;

        h_vel_x[i] = -v_circ * sinf(angle);
        h_vel_y[i] = v_circ * cosf(angle);
        h_vel_z[i] = 0.0f;
    }

    float* d_pos_x, * d_pos_y, * d_pos_z;
    float* d_vel_x, * d_vel_y, * d_vel_z;
    float* d_out_x, * d_out_y, * d_out_z;

    cudaMalloc(&d_pos_x, N * sizeof(float));
    cudaMalloc(&d_pos_y, N * sizeof(float));
    cudaMalloc(&d_pos_z, N * sizeof(float));

    cudaMalloc(&d_vel_x, N * sizeof(float));
    cudaMalloc(&d_vel_y, N * sizeof(float));
    cudaMalloc(&d_vel_z, N * sizeof(float));

    cudaMalloc(&d_out_x, num_outputs * N * sizeof(float));
    cudaMalloc(&d_out_y, num_outputs * N * sizeof(float));
    cudaMalloc(&d_out_z, num_outputs * N * sizeof(float));


    cudaMemcpy(d_pos_x, h_pos_x, N * sizeof(float), cudaMemcpyHostToDevice);
    cudaMemcpy(d_pos_y, h_pos_y, N * sizeof(float), cudaMemcpyHostToDevice);
    cudaMemcpy(d_pos_z, h_pos_z, N * sizeof(float), cudaMemcpyHostToDevice);

    cudaMemcpy(d_vel_x, h_vel_x, N * sizeof(float), cudaMemcpyHostToDevice);
    cudaMemcpy(d_vel_y, h_vel_y, N * sizeof(float), cudaMemcpyHostToDevice);
    cudaMemcpy(d_vel_z, h_vel_z, N * sizeof(float), cudaMemcpyHostToDevice);

    int threads = 256;
    int blocks = (N + threads - 1) / threads;

    cudaEvent_t start, stop;
    cudaEventCreate(&start);
    cudaEventCreate(&stop);

    cudaEventRecord(start);

    propagate << <blocks, threads >> > (
        d_pos_x, d_pos_y, d_pos_z,
        d_vel_x, d_vel_y, d_vel_z,
        d_out_x, d_out_y, d_out_z,
        dt, mu, N, steps, output_interval);

    cudaEventRecord(stop);
    cudaEventSynchronize(stop);

    float ms;
    cudaEventElapsedTime(&ms, start, stop);


    cout << "Kernel time: " << ms << " ms" << endl;

    cudaMemcpy(h_out_x, d_out_x, num_outputs * N * sizeof(float), cudaMemcpyDeviceToHost);
    cudaMemcpy(h_out_y, d_out_y, num_outputs * N * sizeof(float), cudaMemcpyDeviceToHost);
    cudaMemcpy(h_out_z, d_out_z, num_outputs * N * sizeof(float), cudaMemcpyDeviceToHost);

    auto start_writing = high_resolution_clock::now();
    int num_threads = omp_get_max_threads();
    if (num_outputs < num_threads * 2) {
        omp_set_num_threads(num_outputs);
    }

#pragma omp parallel for
    for (int i = 0; i < num_outputs; i++) {
        write_positions(i * output_interval, N,
            &h_out_x[i * N],
            &h_out_y[i * N],
            &h_out_z[i * N]);
    }
    auto end_writing = high_resolution_clock::now();
    milliseconds t_writing = duration_cast<milliseconds>(end_writing - start_writing);

    cout << "Frame writing time: " << t_writing.count() << " ms" << endl;

    cudaFree(d_pos_x);
    cudaFree(d_pos_y);
    cudaFree(d_pos_z);
    cudaFree(d_vel_x);
    cudaFree(d_vel_y);
    cudaFree(d_vel_z);

    cudaFree(d_out_x);
    cudaFree(d_out_y);
    cudaFree(d_out_z);

    delete[] h_pos_x;
    delete[] h_pos_y;
    delete[] h_pos_z;
    delete[] h_vel_x;
    delete[] h_vel_y;
    delete[] h_vel_z;

    delete[] h_out_x;
    delete[] h_out_y;
    delete[] h_out_z;

    return 0;
}
