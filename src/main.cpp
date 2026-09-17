#include <filesystem>
#include <fstream>
#include <iostream>
#include <diesel/modern/massunit.h>
#include <diesel/modern/enginedata.h>
#include <nlohmann/json.hpp>

using namespace diesel;
using namespace diesel::modern;

using json = nlohmann::json;
namespace fs = std::filesystem;

namespace nlohmann {
    void from_json(const nlohmann::json& j, MassUnitResource::InstanceData& p) {
        auto pos = j["pos"].get<std::vector<float>>();
        p.pos = { pos[0], pos[1], pos[2]};

        auto rot = j["rot"].get<std::vector<float>>();
        p.rot = { rot[0], rot[1], rot[2], rot[3]};
    }
}

int main(int argc, char **argv) {
    MassUnitResource mur;

    if (argc < 3) {
        std::cout << "Not enough arguments supplied. You must provide the path to the massunit JSON and the output path." << std::endl;
        return 1;
    }

    if (!fs::exists(argv[1])) {
        std::cout << "JSON File doesn't exist! Cannot create massunit" << std::endl;
        return 1;
    }

    std::cout << "Reading JSON " << argv[1] << std::endl;

    std::ifstream f(argv[1]);
    json data = json::parse(f);

    try
    {
        for (auto& element : data) {
            mur._types.push_back({
                Idstring(std::byteswap(std::stoull(element["unit_type"].get<std::string>(), nullptr, 16))),
                element["pool_size"].get<int>(),
                element["instance_data"].get<std::vector<MassUnitResource::InstanceData>>()
            });
        }
    }
    catch (const std::exception& e)
    {
        std::cerr << "Failed to parse JSON: " << e.what() << std::endl;
        return 1;
    }

    std::cout << "Saving massunit in " << argv[2] << std::endl;

    Writer write(argv[2]);

    mur.Write(write, diesel::EngineVersion::DIESEL_V3);

    return 0;
}
