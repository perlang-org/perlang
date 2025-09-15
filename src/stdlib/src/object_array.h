#pragma once

#include <memory>
#include <initializer_list>

#include "object.h"

namespace perlang
{
    // A class for representing mutable, fixed-size arrays of perlang::Object instances, or anything deriving from it.
    class ObjectArray
    {
     public:
        // Creates a new ObjectArray from a copied array of objects. Note that the objects themselves are not copied;
        // because of the use of shared pointers, ownership will be handled nicely.
        ObjectArray(std::initializer_list<std::shared_ptr<const perlang::Object>> arr);

        // Creates a new ObjectArray of the given size.
        explicit ObjectArray(size_t length);

        ~ObjectArray();

        std::shared_ptr<const perlang::Object> operator[](size_t index) const;

        // C++ doesn't support operator overloading for assignment. We work around this by just providing a set method
        // instead.
        void set(size_t index, const std::shared_ptr<perlang::Object>& value);

        // The length of the array.
        [[nodiscard]]
        size_t length() const;

     private:
        std::shared_ptr<const perlang::Object>* arr_;
        size_t length_;
        bool owned_;
    };
}
